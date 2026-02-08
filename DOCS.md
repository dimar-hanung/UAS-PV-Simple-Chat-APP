# Dokumentasi Project SimpleChatUI

## Daftar Isi
1. [Gambaran Umum](#gambaran-umum)
2. [Struktur Project](#struktur-project)
3. [Arsitektur Aplikasi](#arsitektur-aplikasi)
4. [Komponen UI](#komponen-ui)
5. [Penjelasan Kode](#penjelasan-kode)
   - [Variabel Global](#variabel-global)
   - [Manajemen Konfigurasi](#manajemen-konfigurasi)
   - [Penanganan File](#penanganan-file)
   - [Integrasi API](#integrasi-api)
   - [Markdown Rendering](#markdown-rendering)
   - [UI Helpers](#ui-helpers)
6. [Alur Program](#alur-program)
7. [Kriteria Pemrograman](#kriteria-pemrograman)

---

## Gambaran Umum

**SimpleChatUI** adalah aplikasi desktop Windows Forms yang dibuat menggunakan VB.NET dengan .NET 8.0. Aplikasi ini berfungsi sebagai antarmuka chat yang terintegrasi dengan OpenRouter API untuk berkomunikasi dengan model AI (Google Gemini 3 Pro).

### Fitur Utama:
- Chat dengan AI menggunakan OpenRouter API
- Upload file teks (.txt, .csv, .log, .md, .json, .xml, .html, .css, .js, .vb, .cs)
- Upload gambar (.png, .jpg, .jpeg, .gif, .webp) untuk analisis multimodal
- Streaming response secara real-time
- Penyimpanan API key otomatis
- Rendering markdown pada response AI

---

## Struktur Project

```
UAS-PV/
├── README.md                    # Dokumentasi utama (Bahasa Indonesia)
├── DOCS.md                      # Dokumentasi teknis (file ini)
├── UAS-PV.sln                   # Solution file Visual Studio
├── .gitignore                   # Konfigurasi Git ignore
│
└── SimpleChatUI/                # Folder project utama
    ├── Form1.vb                 # Logic aplikasi utama (620 baris)
    ├── Form1.Designer.vb        # Definisi komponen UI (auto-generated)
    ├── Form1.resx               # Resource file
    ├── ApplicationEvents.vb     # Event lifecycle aplikasi
    ├── SimpleChatUI.vbproj      # Project file MSBuild
    │
    └── My Project/              # Konfigurasi project VB.NET
        ├── Application.Designer.vb
        └── Application.myapp
```

---

## Arsitektur Aplikasi

### Technology Stack
| Komponen | Teknologi |
|----------|-----------|
| Bahasa | Visual Basic .NET (VB.NET) |
| Framework | .NET 8.0 |
| UI Framework | Windows Forms |
| HTTP Client | System.Net.Http.HttpClient |
| JSON Parser | System.Text.Json |
| API | OpenRouter API (Google Gemini 3 Pro) |

### Diagram Arsitektur

```
┌─────────────────────────────────────────────────────────┐
│                    SimpleChatUI                          │
├─────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │   UI Layer  │  │  Business   │  │   Data      │     │
│  │  (WinForms) │◄─│   Logic     │◄─│   Layer     │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
│        │                │                │               │
│        ▼                ▼                ▼               │
│  ┌─────────────────────────────────────────────────┐   │
│  │              Form1.vb (Main Controller)          │   │
│  │  - Event Handlers                                │   │
│  │  - API Integration                               │   │
│  │  - File Processing                               │   │
│  │  - Markdown Rendering                            │   │
│  └─────────────────────────────────────────────────┘   │
│                          │                              │
│                          ▼                              │
│  ┌─────────────────────────────────────────────────┐   │
│  │                External Services                 │   │
│  │  - OpenRouter API (AI Chat)                      │   │
│  │  - Local File System (config.txt)                │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

---

## Komponen UI

Komponen UI didefinisikan di `Form1.Designer.vb`:

| Komponen | Tipe | Fungsi |
|----------|------|--------|
| `txtApiKey` | TextBox | Input API key (masked dengan password char) |
| `btnSaveKey` | Button | Menyimpan API key ke file config |
| `rtbChatDisplay` | RichTextBox | Area tampilan chat (read-only, font Consolas) |
| `btnClear` | Button | Membersihkan history chat |
| `lblAttachment` | Label | Menampilkan status attachment saat ini |
| `btnUpload` | Button | Upload file teks atau gambar |
| `picPreview` | PictureBox | Preview gambar yang di-upload |
| `txtMessage` | TextBox | Input pesan user |
| `btnSend` | Button | Mengirim pesan ke AI |

### Layout Form

```
┌─────────────────────────────────────────────────────┐
│  [txtApiKey................] [btnSaveKey]           │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌─────────────────────────────────────────────┐   │
│  │                                             │   │
│  │            rtbChatDisplay                   │   │
│  │         (Chat History Area)                 │   │
│  │                                             │   │
│  └─────────────────────────────────────────────┘   │
│                                                     │
│  [btnClear]                                         │
│                                                     │
│  lblAttachment: (none)     [picPreview]             │
│                                                     │
│  [btnUpload]                                        │
│                                                     │
│  [txtMessage........................] [btnSend]     │
└─────────────────────────────────────────────────────┘
```

---

## Penjelasan Kode

### Variabel Global

**Lokasi:** `Form1.vb` baris 8-20

```vb
Private ReadOnly httpClient As New HttpClient()
Private uploadedFileContent As String = ""
Private uploadedFileName As String = ""
Private uploadedImageBase64 As String = ""
Private uploadedImageName As String = ""
Private uploadedImageMimeType As String = ""
Private ReadOnly configFilePath As String = Path.Combine(Application.StartupPath, "config.txt")
Private currentAiResponse As New StringBuilder()
Private isFirstStreamChunk As Boolean = True

' Supported file extensions
Private ReadOnly imageExtensions As String() = {".png", ".jpg", ".jpeg", ".gif", ".webp"}
Private ReadOnly textExtensions As String() = {".txt", ".csv", ".log", ".md", ".json", ".xml", ".html", ".css", ".js", ".vb", ".cs"}
```

| Variabel | Fungsi |
|----------|--------|
| `httpClient` | Instance HttpClient untuk HTTP requests |
| `uploadedFileContent` | Menyimpan konten file teks yang di-upload |
| `uploadedFileName` | Nama file teks yang di-upload |
| `uploadedImageBase64` | Gambar dalam format Base64 |
| `uploadedImageName` | Nama file gambar yang di-upload |
| `uploadedImageMimeType` | MIME type gambar (image/png, image/jpeg, dll) |
| `configFilePath` | Path ke file konfigurasi (config.txt) |
| `currentAiResponse` | Buffer untuk mengumpulkan streaming response |
| `isFirstStreamChunk` | Flag untuk menandai chunk pertama response |
| `imageExtensions` | Array ekstensi file gambar yang didukung |
| `textExtensions` | Array ekstensi file teks yang didukung |

---

### Manajemen Konfigurasi

#### SaveConfig (Baris 65-71)
```vb
Private Sub SaveConfig(apiKey As String)
    Try
        File.WriteAllText(configFilePath, apiKey)
    Catch ex As Exception
        MessageBox.Show("Error saving config: " & ex.Message, "Error", ...)
    End Try
End Sub
```

**Fungsi:** Menyimpan API key ke file `config.txt`  
**Kriteria:** File I/O (write)

#### LoadConfig (Baris 77-86)
```vb
Private Sub LoadConfig()
    If File.Exists(configFilePath) Then
        Try
            txtApiKey.Text = File.ReadAllText(configFilePath)
        Catch ex As Exception
            MessageBox.Show("Error loading config: " & ex.Message, ...)
        End Try
    End If
End Sub
```

**Fungsi:** Memuat API key dari file `config.txt` saat aplikasi dimulai  
**Kriteria:** File I/O (read), Branching (If file exists)

---

### Penanganan File

#### UploadFile (Baris 92-123)
```vb
Private Sub UploadFile()
    Using openFileDialog As New OpenFileDialog()
        openFileDialog.Filter = "All supported files|*.txt;*.csv;..."
        
        If openFileDialog.ShowDialog() = DialogResult.OK Then
            Dim extension = Path.GetExtension(filePath).ToLower()
            
            ' Branching: Detect file type
            If IsImageFile(extension) Then
                ProcessImageFile(filePath, fileName, extension)
            ElseIf IsTextFile(extension) Then
                ProcessTextFile(filePath, fileName)
            Else
                ProcessTextFile(filePath, fileName) ' Default: treat as text
            End If
        End If
    End Using
End Sub
```

**Fungsi:** Membuka dialog file dan mendeteksi tipe file secara otomatis  
**Kriteria:** File I/O (OpenFileDialog), Branching (if image/text)

#### IsImageFile & IsTextFile (Baris 129-153)
```vb
Private Function IsImageFile(extension As String) As Boolean
    For Each imgExt In imageExtensions
        If extension = imgExt Then Return True
    Next
    Return False
End Function
```

**Fungsi:** Mengecek apakah ekstensi file adalah gambar atau teks  
**Kriteria:** Looping (For Each), Branching (If)

#### ProcessImageFile (Baris 159-185)
```vb
Private Sub ProcessImageFile(filePath As String, fileName As String, extension As String)
    ' Read image as bytes dan convert ke base64
    Dim imageBytes As Byte() = File.ReadAllBytes(filePath)
    uploadedImageBase64 = Convert.ToBase64String(imageBytes)
    
    ' Determine MIME type
    If extension = ".png" Then
        uploadedImageMimeType = "image/png"
    ElseIf extension = ".jpg" OrElse extension = ".jpeg" Then
        uploadedImageMimeType = "image/jpeg"
    ' ... dst
    End If
    
    ' Show preview
    picPreview.Image = Image.FromFile(filePath)
    picPreview.Visible = True
End Sub
```

**Fungsi:** Memproses file gambar - membaca bytes, convert ke Base64, menentukan MIME type, dan menampilkan preview  
**Kriteria:** File I/O (ReadAllBytes), Branching (MIME type detection)

#### ProcessTextFile (Baris 191-210)
```vb
Private Sub ProcessTextFile(filePath As String, fileName As String)
    Dim lines As String() = File.ReadAllLines(filePath)
    Dim sb As New StringBuilder()
    
    ' Looping: For Each line
    For Each line As String In lines
        sb.AppendLine(line)
    Next
    
    uploadedFileContent = sb.ToString()
    uploadedFileName = fileName
End Sub
```

**Fungsi:** Membaca file teks baris per baris  
**Kriteria:** File I/O (ReadAllLines), Looping (For Each line)

---

### Integrasi API

#### SendMessage (Baris 238-284)
```vb
Private Async Sub SendMessage()
    Dim apiKey As String = txtApiKey.Text.Trim()
    Dim message As String = txtMessage.Text.Trim()
    
    ' Validasi input
    If apiKey = "" Then
        MessageBox.Show("Please enter your API key first.", ...)
        Return
    End If
    
    If message = "" Then
        MessageBox.Show("Please enter a message.", ...)
        Return
    End If
    
    ' Disable controls & call API
    SetControlsEnabled(False)
    Await CallOpenRouter(apiKey, message, uploadedFileContent, ...)
    SetControlsEnabled(True)
End Sub
```

**Fungsi:** Validasi input dan memulai API call  
**Kriteria:** Branching (If validasi)

#### CallOpenRouter (Baris 290-367)
```vb
Private Async Function CallOpenRouter(apiKey As String, prompt As String, ...) As Task
    ' Build content array untuk multimodal support
    Dim contentParts As New List(Of Object)
    
    ' Add text content
    contentParts.Add(New With {
        .type = "text",
        .text = textContent
    })
    
    ' Add image jika ada
    If imageBase64 <> "" Then
        contentParts.Add(New With {
            .type = "image_url",
            .image_url = New With {
                .url = $"data:{imageMimeType};base64,{imageBase64}"
            }
        })
    End If
    
    ' Prepare request body
    Dim requestBody = New With {
        .model = "google/gemini-3-pro-preview",
        .messages = New Object() { ... },
        .stream = True
    }
    
    ' Send request dengan streaming
    Using response = Await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
        If response.IsSuccessStatusCode Then
            Using reader As New StreamReader(stream)
                ' Looping: While reading streaming response
                Dim line As String = Await reader.ReadLineAsync()
                While line IsNot Nothing
                    ProcessStreamLine(line)
                    line = Await reader.ReadLineAsync()
                End While
            End Using
        End If
    End Using
End Function
```

**Fungsi:** Melakukan HTTP POST ke OpenRouter API dengan streaming response  
**Kriteria:** Looping (While streaming), Branching (status check, content type)

**Endpoint API:** `https://openrouter.ai/api/v1/chat/completions`  
**Model:** `google/gemini-3-pro-preview`

#### ProcessStreamLine (Baris 372-408)
```vb
Private Sub ProcessStreamLine(line As String)
    If line.StartsWith("data: ") Then
        Dim jsonData = line.Substring(6).Trim()
        
        If jsonData = "[DONE]" Then Return
        
        ' Parse JSON dan extract content
        Using doc = JsonDocument.Parse(jsonData)
            Dim content = doc.RootElement
                .GetProperty("choices")(0)
                .GetProperty("delta")
                .GetProperty("content")
                .GetString()
            
            currentAiResponse.Append(content)
            Me.Invoke(Sub() AppendStreamingText(content))
        End Using
    End If
End Sub
```

**Fungsi:** Memproses setiap baris dari streaming response (Server-Sent Events/SSE format)

---

### Markdown Rendering

#### RenderMarkdownResponse (Baris 429-477)
```vb
Private Sub RenderMarkdownInternal(markdownText As String)
    Dim lines = markdownText.Split({vbCrLf, vbLf}, StringSplitOptions.None)
    Dim inCodeBlock As Boolean = False
    
    For Each line As String In lines
        ' Check code block
        If line.StartsWith("```") Then
            If Not inCodeBlock Then
                inCodeBlock = True
                codeBlockLang = line.Substring(3).Trim()
            Else
                inCodeBlock = False
                RenderCodeBlock(codeBlockContent.ToString(), codeBlockLang)
            End If
        ElseIf inCodeBlock Then
            codeBlockContent.AppendLine(line)
        Else
            RenderMarkdownLine(line)
        End If
    Next
End Sub
```

**Fungsi:** Parsing dan rendering markdown lengkap dengan code blocks

#### RenderMarkdownLine (Baris 482-508)
```vb
Private Sub RenderMarkdownLine(line As String)
    ' Headers
    If line.StartsWith("### ") Then
        AppendFormattedText(line.Substring(4), Color.DarkBlue, FontStyle.Bold)
        Return
    ElseIf line.StartsWith("## ") Then ...
    
    ' Bullet points
    Dim bulletMatch = Regex.Match(line, "^(\s*)([-*+]|\d+\.)\s+(.*)$")
    If bulletMatch.Success Then ...
    
    ' Regular line
    RenderInlineMarkdown(line)
End Sub
```

**Fungsi:** Rendering baris markdown individual (headers, bullets)

#### RenderInlineMarkdown (Baris 514-549)
```vb
Private Sub RenderInlineMarkdown(text As String)
    ' Pattern: **bold**, *italic*, `code`, [link](url)
    Dim pattern As String = "(\*\*(.+?)\*\*|\*(.+?)\*|`(.+?)`|\[(.+?)\]\((.+?)\))"
    Dim matches = Regex.Matches(text, pattern)
    
    For Each match As Match In matches
        If match.Groups(2).Success Then
            ' Bold: **text**
            AppendFormattedText(match.Groups(2).Value, Color.Black, FontStyle.Bold)
        ElseIf match.Groups(3).Success Then
            ' Italic: *text*
            AppendFormattedText(match.Groups(3).Value, Color.Black, FontStyle.Italic)
        ElseIf match.Groups(4).Success Then
            ' Inline code: `code`
            AppendFormattedText(match.Groups(4).Value, Color.DarkRed, FontStyle.Regular, Color.LightGray)
        ElseIf match.Groups(5).Success Then
            ' Link: [text](url)
            AppendFormattedText(match.Groups(5).Value, Color.Blue, FontStyle.Underline)
        End If
    Next
End Sub
```

**Fungsi:** Parsing dan rendering inline markdown (bold, italic, code, links)  
**Regex Pattern:** `(\*\*(.+?)\*\*|\*(.+?)\*|`(.+?)`|\[(.+?)\]\((.+?)\))`

| Format | Syntax | Contoh |
|--------|--------|--------|
| Bold | `**text**` | **bold text** |
| Italic | `*text*` | *italic text* |
| Code | `` `code` `` | `inline code` |
| Link | `[text](url)` | [link text](url) |

---

### UI Helpers

#### AppendFormattedText (Baris 570-582)
```vb
Private Sub AppendFormattedText(text As String, foreColor As Color, style As FontStyle, 
                                 Optional backColor As Color = Nothing)
    Dim startPos = rtbChatDisplay.TextLength
    rtbChatDisplay.AppendText(text)
    rtbChatDisplay.Select(startPos, text.Length)
    rtbChatDisplay.SelectionColor = foreColor
    rtbChatDisplay.SelectionBackColor = backColor
    rtbChatDisplay.SelectionFont = New Font(rtbChatDisplay.Font, style)
End Sub
```

**Fungsi:** Menambahkan teks dengan formatting (warna, style) ke RichTextBox

#### SetControlsEnabled (Baris 611-617)
```vb
Private Sub SetControlsEnabled(enabled As Boolean)
    btnSend.Enabled = enabled
    btnUpload.Enabled = enabled
    txtMessage.Enabled = enabled
    txtApiKey.Enabled = enabled
    btnSaveKey.Enabled = enabled
End Sub
```

**Fungsi:** Enable/disable semua kontrol saat API call sedang berjalan

#### ScrollToEnd (Baris 603-606)
```vb
Private Sub ScrollToEnd()
    rtbChatDisplay.SelectionStart = rtbChatDisplay.TextLength
    rtbChatDisplay.ScrollToCaret()
End Sub
```

**Fungsi:** Auto-scroll chat display ke bagian bawah

---

## Alur Program

```
┌─────────────────┐
│  Form1_Load     │
│  (Load Config)  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  User Input:    │
│  - Enter API Key│
│  - Type Message │
│  - Upload File  │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│         btnSend_Click / Enter Key       │
├─────────────────────────────────────────┤
│  1. Validasi API Key                    │
│  2. Validasi Message                    │
│  3. Display user message                │
│  4. Disable controls                    │
│  5. Call OpenRouter API                 │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│           CallOpenRouter()              │
├─────────────────────────────────────────┤
│  1. Build content array (text + image)  │
│  2. Serialize to JSON                   │
│  3. Send HTTP POST with streaming       │
│  4. Process streaming response          │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│        ProcessStreamLine()              │
├─────────────────────────────────────────┤
│  - Parse SSE data                       │
│  - Extract content from JSON            │
│  - Append to display in real-time       │
└────────┬────────────────────────────────┘
         │
         ▼
┌─────────────────┐
│  Re-enable      │
│  Controls       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Clear Upload   │
│  State          │
└─────────────────┘
```

---

## Kriteria Pemrograman

Sesuai dengan persyaratan UAS Pemrograman Visual, program ini **wajib memenuhi 4 kriteria utama**:

| No | Kriteria | Status | Keterangan |
|----|----------|--------|------------|
| a | Program dapat membaca file | ✅ Terpenuhi | Membaca config.txt, file teks, dan file gambar |
| b | Terdapat percabangan di dalam program | ✅ Terpenuhi | If-Then-Else untuk validasi dan logika |
| c | Terdapat perulangan di dalam program | ✅ Terpenuhi | For Each dan While untuk iterasi data |
| d | Terdapat procedure di dalam program | ✅ Terpenuhi | 20+ Sub dan Function terpisah |

---

### Kriteria A: Program Dapat Membaca File

Program ini mengimplementasikan pembacaan file dalam beberapa cara:

#### 1. Membaca File Konfigurasi (`LoadConfig`)
**Lokasi:** `Form1.vb` baris 77-86

```vb
Private Sub LoadConfig()
    ' Membaca API key dari file config.txt
    If File.Exists(configFilePath) Then
        Try
            txtApiKey.Text = File.ReadAllText(configFilePath)
        Catch ex As Exception
            MessageBox.Show("Error loading config: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End If
End Sub
```

**Penjelasan:**
- Menggunakan `File.ReadAllText()` untuk membaca seluruh isi file sekaligus
- File yang dibaca: `config.txt` (berisi API key)
- Dipanggil saat aplikasi pertama kali dijalankan (`Form1_Load`)

#### 2. Membaca File Teks (`ProcessTextFile`)
**Lokasi:** `Form1.vb` baris 191-210

```vb
Private Sub ProcessTextFile(filePath As String, fileName As String)
    ' Membaca file teks baris per baris
    Dim lines As String() = File.ReadAllLines(filePath)
    Dim sb As New StringBuilder()

    For Each line As String In lines
        sb.AppendLine(line)
    Next

    uploadedFileContent = sb.ToString()
    uploadedFileName = fileName
    lblAttachment.Text = "Text: " & fileName
End Sub
```

**Penjelasan:**
- Menggunakan `File.ReadAllLines()` untuk membaca file sebagai array baris
- Mendukung berbagai format: `.txt`, `.csv`, `.log`, `.md`, `.json`, `.xml`, `.html`, `.css`, `.js`, `.vb`, `.cs`
- Konten file disimpan untuk dikirim ke AI

#### 3. Membaca File Gambar (`ProcessImageFile`)
**Lokasi:** `Form1.vb` baris 159-185

```vb
Private Sub ProcessImageFile(filePath As String, fileName As String, extension As String)
    ' Membaca file gambar sebagai bytes
    Dim imageBytes As Byte() = File.ReadAllBytes(filePath)
    uploadedImageBase64 = Convert.ToBase64String(imageBytes)
    uploadedImageName = fileName
    
    ' ... menentukan MIME type ...
    
    ' Menampilkan preview gambar
    picPreview.Image = Image.FromFile(filePath)
    picPreview.Visible = True
End Sub
```

**Penjelasan:**
- Menggunakan `File.ReadAllBytes()` untuk membaca file gambar sebagai byte array
- Menggunakan `Image.FromFile()` untuk menampilkan preview
- Mendukung format: `.png`, `.jpg`, `.jpeg`, `.gif`, `.webp`

#### Ringkasan Pembacaan File

| Method | Fungsi VB.NET | Tipe File | Kegunaan |
|--------|---------------|-----------|----------|
| `LoadConfig()` | `File.ReadAllText()` | config.txt | Memuat API key tersimpan |
| `ProcessTextFile()` | `File.ReadAllLines()` | .txt, .csv, .md, dll | Upload konten teks ke AI |
| `ProcessImageFile()` | `File.ReadAllBytes()` | .png, .jpg, .gif, dll | Upload gambar ke AI |
| `ProcessImageFile()` | `Image.FromFile()` | .png, .jpg, .gif, dll | Preview gambar |

---

### Kriteria B: Terdapat Percabangan di Dalam Program

Program ini menggunakan percabangan `If-Then-Else` secara ekstensif:

#### 1. Percabangan untuk Validasi Input (`SendMessage`)
**Lokasi:** `Form1.vb` baris 238-284

```vb
Private Async Sub SendMessage()
    Dim apiKey As String = txtApiKey.Text.Trim()
    Dim message As String = txtMessage.Text.Trim()

    ' Percabangan 1: Validasi API Key
    If apiKey = "" Then
        MessageBox.Show("Please enter your API key first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Return
    End If

    ' Percabangan 2: Validasi Message
    If message = "" Then
        MessageBox.Show("Please enter a message.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Return
    End If

    ' Percabangan 3: Menampilkan info attachment
    Dim displayMessage = "User: " & message
    If uploadedImageName <> "" Then
        displayMessage &= " [Image: " & uploadedImageName & "]"
    ElseIf uploadedFileName <> "" Then
        displayMessage &= " [File: " & uploadedFileName & "]"
    End If
    
    ' ... lanjutan kode ...
End Sub
```

#### 2. Percabangan untuk Deteksi Tipe File (`UploadFile`)
**Lokasi:** `Form1.vb` baris 92-123

```vb
Private Sub UploadFile()
    Using openFileDialog As New OpenFileDialog()
        ' ...
        If openFileDialog.ShowDialog() = DialogResult.OK Then
            Dim extension = Path.GetExtension(filePath).ToLower()
            
            ' Percabangan: Menentukan tipe file berdasarkan ekstensi
            If IsImageFile(extension) Then
                ' Proses sebagai gambar
                ProcessImageFile(filePath, fileName, extension)
            ElseIf IsTextFile(extension) Then
                ' Proses sebagai teks
                ProcessTextFile(filePath, fileName)
            Else
                ' Default: coba baca sebagai teks
                ProcessTextFile(filePath, fileName)
            End If
        End If
    End Using
End Sub
```

#### 3. Percabangan untuk Menentukan MIME Type (`ProcessImageFile`)
**Lokasi:** `Form1.vb` baris 165-176

```vb
' Percabangan untuk menentukan MIME type gambar
If extension = ".png" Then
    uploadedImageMimeType = "image/png"
ElseIf extension = ".jpg" OrElse extension = ".jpeg" Then
    uploadedImageMimeType = "image/jpeg"
ElseIf extension = ".gif" Then
    uploadedImageMimeType = "image/gif"
ElseIf extension = ".webp" Then
    uploadedImageMimeType = "image/webp"
Else
    uploadedImageMimeType = "image/png" ' Default
End If
```

#### 4. Percabangan untuk Cek Ekstensi File (`IsImageFile`)
**Lokasi:** `Form1.vb` baris 129-138

```vb
Private Function IsImageFile(extension As String) As Boolean
    For Each imgExt In imageExtensions
        ' Percabangan: Cek apakah ekstensi cocok
        If extension = imgExt Then
            Return True
        End If
    Next
    Return False
End Function
```

#### 5. Percabangan untuk Response API (`CallOpenRouter`)
**Lokasi:** `Form1.vb` baris 341-361

```vb
' Percabangan: Cek apakah ada konten file
If fileContent <> "" Then
    textContent = "[File Content]" & vbCrLf & fileContent & vbCrLf & vbCrLf & "[Message]" & vbCrLf & prompt
End If

' Percabangan: Cek apakah ada gambar
If imageBase64 <> "" Then
    contentParts.Add(New With {
        .type = "image_url",
        .image_url = New With {
            .url = $"data:{imageMimeType};base64,{imageBase64}"
        }
    })
End If

' Percabangan: Cek status response
If response.IsSuccessStatusCode Then
    ' Proses streaming response
Else
    ' Tampilkan error
    Dim errorContent = Await response.Content.ReadAsStringAsync()
    AppendPlainText("[Error] API returned status " & CInt(response.StatusCode) & ": " & errorContent, Color.Red)
End If
```

#### 6. Percabangan untuk Parsing Markdown (`RenderMarkdownLine`)
**Lokasi:** `Form1.vb` baris 482-508

```vb
Private Sub RenderMarkdownLine(line As String)
    ' Percabangan: Deteksi header markdown
    If line.StartsWith("### ") Then
        AppendFormattedText(line.Substring(4), Color.DarkBlue, FontStyle.Bold)
        Return
    ElseIf line.StartsWith("## ") Then
        AppendFormattedText(line.Substring(3), Color.DarkBlue, FontStyle.Bold)
        Return
    ElseIf line.StartsWith("# ") Then
        AppendFormattedText(line.Substring(2), Color.DarkBlue, FontStyle.Bold)
        Return
    End If

    ' Percabangan: Deteksi bullet points
    Dim bulletMatch = Regex.Match(line, "^(\s*)([-*+]|\d+\.)\s+(.*)$")
    If bulletMatch.Success Then
        ' Render bullet point
        ' ...
        Return
    End If

    ' Default: render sebagai teks biasa
    RenderInlineMarkdown(line)
End Sub
```

#### Ringkasan Percabangan

| Lokasi | Jenis Percabangan | Tujuan |
|--------|-------------------|--------|
| `SendMessage()` | If-Then | Validasi API key dan message |
| `UploadFile()` | If-ElseIf-Else | Deteksi tipe file (gambar/teks) |
| `ProcessImageFile()` | If-ElseIf-Else | Menentukan MIME type |
| `IsImageFile()` | If-Then | Cek kecocokan ekstensi |
| `IsTextFile()` | If-Then | Cek kecocokan ekstensi |
| `CallOpenRouter()` | If-Then | Cek konten file dan gambar |
| `CallOpenRouter()` | If-Else | Cek status response API |
| `ProcessStreamLine()` | If-Then | Parsing data streaming |
| `RenderMarkdownLine()` | If-ElseIf-Else | Parsing format markdown |
| `RenderInlineMarkdown()` | If-ElseIf | Parsing inline formatting |

---

### Kriteria C: Terdapat Perulangan di Dalam Program

Program ini menggunakan perulangan `For Each` dan `While`:

#### 1. Perulangan For Each - Cek Ekstensi Gambar (`IsImageFile`)
**Lokasi:** `Form1.vb` baris 129-138

```vb
Private Function IsImageFile(extension As String) As Boolean
    ' Perulangan: Iterasi setiap ekstensi gambar yang didukung
    For Each imgExt In imageExtensions
        If extension = imgExt Then
            Return True
        End If
    Next
    Return False
End Function
```

**Penjelasan:** Mengiterasi array `{".png", ".jpg", ".jpeg", ".gif", ".webp"}` untuk mengecek apakah file adalah gambar.

#### 2. Perulangan For Each - Cek Ekstensi Teks (`IsTextFile`)
**Lokasi:** `Form1.vb` baris 144-153

```vb
Private Function IsTextFile(extension As String) As Boolean
    ' Perulangan: Iterasi setiap ekstensi teks yang didukung
    For Each txtExt In textExtensions
        If extension = txtExt Then
            Return True
        End If
    Next
    Return False
End Function
```

**Penjelasan:** Mengiterasi array `{".txt", ".csv", ".log", ".md", ".json", ".xml", ".html", ".css", ".js", ".vb", ".cs"}`.

#### 3. Perulangan For Each - Membaca Baris File (`ProcessTextFile`)
**Lokasi:** `Form1.vb` baris 196-199

```vb
Dim lines As String() = File.ReadAllLines(filePath)
Dim sb As New StringBuilder()

' Perulangan: Iterasi setiap baris dalam file
For Each line As String In lines
    sb.AppendLine(line)
Next
```

**Penjelasan:** Memproses setiap baris dari file teks yang di-upload.

#### 4. Perulangan While - Streaming Response API (`CallOpenRouter`)
**Lokasi:** `Form1.vb` baris 346-350

```vb
Using reader As New StreamReader(stream)
    ' Perulangan: Membaca streaming response baris per baris
    Dim line As String = Await reader.ReadLineAsync()
    While line IsNot Nothing
        ProcessStreamLine(line)
        line = Await reader.ReadLineAsync()
    End While
End Using
```

**Penjelasan:** Loop `While` untuk membaca response API secara streaming (Server-Sent Events). Loop berjalan selama masih ada data yang diterima.

#### 5. Perulangan For Each - Rendering Markdown (`RenderMarkdownInternal`)
**Lokasi:** `Form1.vb` baris 447-468

```vb
Dim lines = markdownText.Split({vbCrLf, vbLf}, StringSplitOptions.None)

' Perulangan: Iterasi setiap baris markdown
For Each line As String In lines
    If line.StartsWith("```") Then
        ' Handle code block
    ElseIf inCodeBlock Then
        codeBlockContent.AppendLine(line)
    Else
        RenderMarkdownLine(line)
    End If
Next
```

**Penjelasan:** Memproses setiap baris response AI untuk rendering markdown.

#### 6. Perulangan For Each - Parsing Inline Markdown (`RenderInlineMarkdown`)
**Lokasi:** `Form1.vb` baris 521-543

```vb
Dim matches = Regex.Matches(text, pattern)

' Perulangan: Iterasi setiap regex match
For Each match As Match In matches
    ' Add text before match
    If match.Index > lastIndex Then
        AppendFormattedText(text.Substring(lastIndex, match.Index - lastIndex), Color.Black, FontStyle.Regular)
    End If

    ' Determine format type dan render
    If match.Groups(2).Success Then
        AppendFormattedText(match.Groups(2).Value, Color.Black, FontStyle.Bold)
    ElseIf match.Groups(3).Success Then
        AppendFormattedText(match.Groups(3).Value, Color.Black, FontStyle.Italic)
    ' ... dst
    End If

    lastIndex = match.Index + match.Length
Next
```

**Penjelasan:** Mengiterasi setiap match regex untuk formatting **bold**, *italic*, `code`, dan [link](url).

#### 7. Perulangan For Each - Rendering Code Block (`RenderCodeBlock`)
**Lokasi:** `Form1.vb` baris 561-564

```vb
Dim codeLines = code.TrimEnd().Split({vbCrLf, vbLf}, StringSplitOptions.None)

' Perulangan: Iterasi setiap baris code block
For Each codeLine In codeLines
    AppendFormattedText("  " & codeLine & vbCrLf, Color.DarkGreen, FontStyle.Regular, Color.FromArgb(245, 245, 245))
Next
```

**Penjelasan:** Memproses setiap baris dalam code block untuk rendering dengan background berwarna.

#### Ringkasan Perulangan

| Lokasi | Tipe Loop | Data yang Diiterasi | Tujuan |
|--------|-----------|---------------------|--------|
| `IsImageFile()` | For Each | Array ekstensi gambar | Validasi tipe file |
| `IsTextFile()` | For Each | Array ekstensi teks | Validasi tipe file |
| `ProcessTextFile()` | For Each | Baris-baris file | Membaca konten file |
| `CallOpenRouter()` | While | Stream response | Menerima data real-time |
| `RenderMarkdownInternal()` | For Each | Baris-baris markdown | Parsing markdown |
| `RenderInlineMarkdown()` | For Each | Regex matches | Formatting teks |
| `RenderCodeBlock()` | For Each | Baris-baris code | Rendering code block |

---

### Kriteria D: Terdapat Procedure di Dalam Program

Program ini mengimplementasikan **20+ procedure** (Sub dan Function) yang terpisah dan terorganisir:

#### Daftar Lengkap Procedure

| No | Nama Procedure | Tipe | Lokasi (Baris) | Fungsi |
|----|----------------|------|----------------|--------|
| 1 | `Form1_Load` | Sub | 23-25 | Event handler saat form dimuat |
| 2 | `Form1_FormClosing` | Sub | 28-30 | Event handler saat form ditutup |
| 3 | `btnSaveKey_Click` | Sub | 33-36 | Event handler tombol Save |
| 4 | `btnUpload_Click` | Sub | 39-41 | Event handler tombol Upload |
| 5 | `btnSend_Click` | Sub | 44-46 | Event handler tombol Send |
| 6 | `btnClear_Click` | Sub | 49-51 | Event handler tombol Clear |
| 7 | `txtMessage_KeyDown` | Sub | 54-59 | Event handler keyboard |
| 8 | `SaveConfig` | Sub | 65-71 | Menyimpan API key ke file |
| 9 | `LoadConfig` | Sub | 77-86 | Memuat API key dari file |
| 10 | `UploadFile` | Sub | 92-123 | Membuka dialog upload file |
| 11 | `IsImageFile` | Function | 129-138 | Mengecek ekstensi gambar |
| 12 | `IsTextFile` | Function | 144-153 | Mengecek ekstensi teks |
| 13 | `ProcessImageFile` | Sub | 159-185 | Memproses file gambar |
| 14 | `ProcessTextFile` | Sub | 191-210 | Memproses file teks |
| 15 | `ClearUploadedContent` | Sub | 215-224 | Membersihkan state upload |
| 16 | `ClearChat` | Sub | 229-232 | Membersihkan chat display |
| 17 | `SendMessage` | Async Sub | 238-284 | Mengirim pesan ke API |
| 18 | `CallOpenRouter` | Async Function | 290-367 | Memanggil OpenRouter API |
| 19 | `ProcessStreamLine` | Sub | 372-408 | Memproses streaming response |
| 20 | `AppendStreamingText` | Sub | 413-423 | Menampilkan teks streaming |
| 21 | `RenderMarkdownResponse` | Sub | 429-435 | Entry point rendering markdown |
| 22 | `RenderMarkdownInternal` | Sub | 437-477 | Implementasi rendering markdown |
| 23 | `RenderMarkdownLine` | Sub | 482-508 | Rendering satu baris markdown |
| 24 | `RenderInlineMarkdown` | Sub | 514-549 | Rendering inline formatting |
| 25 | `RenderCodeBlock` | Sub | 554-565 | Rendering code block |
| 26 | `AppendFormattedText` | Sub | 570-582 | Helper formatting RichTextBox |
| 27 | `AppendPlainText` | Sub | 587-593 | Helper teks biasa |
| 28 | `AppendPlainTextInternal` | Sub | 595-598 | Implementasi AppendPlainText |
| 29 | `ScrollToEnd` | Sub | 603-606 | Auto-scroll chat display |
| 30 | `SetControlsEnabled` | Sub | 611-617 | Enable/disable UI controls |

#### Contoh Detail Procedure

**1. Procedure dengan Parameter (`SaveConfig`)**
```vb
''' <summary>
''' Procedure: SaveConfig - Saves API key to config.txt file
''' </summary>
Private Sub SaveConfig(apiKey As String)
    Try
        File.WriteAllText(configFilePath, apiKey)
    Catch ex As Exception
        MessageBox.Show("Error saving config: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Try
End Sub
```

**2. Function dengan Return Value (`IsImageFile`)**
```vb
''' <summary>
''' Function: IsImageFile - Checks if extension is an image type
''' </summary>
Private Function IsImageFile(extension As String) As Boolean
    For Each imgExt In imageExtensions
        If extension = imgExt Then
            Return True
        End If
    Next
    Return False
End Function
```

**3. Async Procedure (`CallOpenRouter`)**
```vb
''' <summary>
''' Procedure: CallOpenRouter - Makes API request with multimodal support
''' </summary>
Private Async Function CallOpenRouter(apiKey As String, prompt As String, 
                                       fileContent As String, imageBase64 As String, 
                                       imageMimeType As String) As Task
    ' ... implementasi async ...
End Function
```

**4. Procedure dengan Optional Parameter (`AppendFormattedText`)**
```vb
''' <summary>
''' Helper: Append formatted text to RichTextBox
''' </summary>
Private Sub AppendFormattedText(text As String, foreColor As Color, 
                                 style As FontStyle, 
                                 Optional backColor As Color = Nothing)
    If backColor = Nothing Then
        backColor = Color.White
    End If
    ' ... implementasi ...
End Sub
```

#### Kategori Procedure

| Kategori | Jumlah | Contoh |
|----------|--------|--------|
| Event Handlers | 7 | `Form1_Load`, `btnSend_Click`, `txtMessage_KeyDown` |
| File Operations | 4 | `SaveConfig`, `LoadConfig`, `ProcessTextFile`, `ProcessImageFile` |
| API Integration | 3 | `SendMessage`, `CallOpenRouter`, `ProcessStreamLine` |
| Markdown Rendering | 5 | `RenderMarkdownResponse`, `RenderMarkdownLine`, `RenderInlineMarkdown` |
| UI Helpers | 6 | `AppendFormattedText`, `ScrollToEnd`, `SetControlsEnabled` |
| Utility | 5 | `IsImageFile`, `IsTextFile`, `ClearChat`, `ClearUploadedContent` |

#### Manfaat Penggunaan Procedure

1. **Modularitas**: Kode terorganisir dalam unit-unit kecil yang mudah dipahami
2. **Reusability**: Procedure dapat dipanggil dari berbagai tempat
3. **Maintainability**: Perubahan hanya perlu dilakukan di satu tempat
4. **Testability**: Setiap procedure dapat diuji secara independen
5. **Readability**: Nama procedure menjelaskan fungsinya

---

### Ringkasan Pemenuhan Kriteria

```
┌─────────────────────────────────────────────────────────────────────┐
│                    PEMENUHAN KRITERIA PROGRAM                        │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ✅ A. Program Dapat Membaca File                                   │
│     └─ 4 implementasi: ReadAllText, ReadAllLines, ReadAllBytes,     │
│        Image.FromFile                                               │
│                                                                     │
│  ✅ B. Terdapat Percabangan                                         │
│     └─ 10+ lokasi dengan If-Then-Else untuk validasi, deteksi       │
│        tipe file, parsing, dan error handling                       │
│                                                                     │
│  ✅ C. Terdapat Perulangan                                          │
│     └─ 7 implementasi: 6x For Each + 1x While untuk iterasi         │
│        data dan streaming                                           │
│                                                                     │
│  ✅ D. Terdapat Procedure                                           │
│     └─ 30 procedure (Sub dan Function) yang terorganisir            │
│        dalam kategori yang jelas                                    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Catatan Keamanan

1. **API Key Storage:** API key disimpan di `config.txt` dalam format plain text. File ini di-exclude dari Git melalui `.gitignore`.

2. **Sensitive Data:** Pastikan untuk tidak membagikan file `config.txt` yang berisi API key.

3. **HTTPS:** Semua komunikasi dengan OpenRouter API menggunakan HTTPS.

---

## Referensi

- [OpenRouter API Documentation](https://openrouter.ai/docs)
- [.NET 8.0 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Windows Forms Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)

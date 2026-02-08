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

Sesuai dengan persyaratan UAS Pemrograman Visual, kode ini mengimplementasikan:

### 1. File I/O

| Operasi | Lokasi | Fungsi |
|---------|--------|--------|
| Write | `SaveConfig()` baris 67 | `File.WriteAllText(configFilePath, apiKey)` |
| Read | `LoadConfig()` baris 81 | `File.ReadAllText(configFilePath)` |
| Read Lines | `ProcessTextFile()` baris 193 | `File.ReadAllLines(filePath)` |
| Read Bytes | `ProcessImageFile()` baris 161 | `File.ReadAllBytes(filePath)` |

### 2. Branching (If Statement)

| Lokasi | Kondisi |
|--------|---------|
| `LoadConfig()` | `If File.Exists(configFilePath)` |
| `UploadFile()` | `If IsImageFile(extension)`, `ElseIf IsTextFile(extension)` |
| `SendMessage()` | `If apiKey = ""`, `If message = ""`, `If uploadedImageName <> ""` |
| `CallOpenRouter()` | `If fileContent <> ""`, `If imageBase64 <> ""`, `If response.IsSuccessStatusCode` |
| `ProcessImageFile()` | `If extension = ".png"`, `ElseIf extension = ".jpg"`, dll |
| `RenderMarkdownLine()` | `If line.StartsWith("### ")`, `ElseIf line.StartsWith("## ")`, dll |
| `RenderInlineMarkdown()` | `If match.Groups(2).Success`, `ElseIf match.Groups(3).Success`, dll |

### 3. Looping (For Each / While)

| Lokasi | Loop Type | Deskripsi |
|--------|-----------|-----------|
| `IsImageFile()` | For Each | Iterasi array ekstensi gambar |
| `IsTextFile()` | For Each | Iterasi array ekstensi teks |
| `ProcessTextFile()` | For Each | Iterasi setiap baris file |
| `CallOpenRouter()` | While | Streaming response dari API |
| `RenderMarkdownInternal()` | For Each | Iterasi setiap baris markdown |
| `RenderInlineMarkdown()` | For Each | Iterasi regex matches |
| `RenderCodeBlock()` | For Each | Iterasi baris code block |

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

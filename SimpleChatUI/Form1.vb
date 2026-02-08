Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.RegularExpressions

Public Class Form1
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

    ' Form Load - Load API key from config
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadConfig()
    End Sub

    ' Form Closing - Save API key to config
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        SaveConfig(txtApiKey.Text)
    End Sub

    ' Save button click
    Private Sub btnSaveKey_Click(sender As Object, e As EventArgs) Handles btnSaveKey.Click
        SaveConfig(txtApiKey.Text)
        MessageBox.Show("API Key saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ' Upload button click - single button for both text and image
    Private Sub btnUpload_Click(sender As Object, e As EventArgs) Handles btnUpload.Click
        UploadFile()
    End Sub

    ' Send button click
    Private Sub btnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
        SendMessage()
    End Sub

    ' Clear button click - clears chat display
    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        ClearChat()
    End Sub

    ' Allow Enter key to send message
    Private Sub txtMessage_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMessage.KeyDown
        If e.KeyCode = Keys.Enter AndAlso Not e.Shift Then
            e.SuppressKeyPress = True
            SendMessage()
        End If
    End Sub

    ''' <summary>
    ''' Procedure: SaveConfig - Saves API key to config.txt file
    ''' Criteria: File I/O (write)
    ''' </summary>
    Private Sub SaveConfig(apiKey As String)
        Try
            File.WriteAllText(configFilePath, apiKey)
        Catch ex As Exception
            MessageBox.Show("Error saving config: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Procedure: LoadConfig - Loads API key from config.txt file
    ''' Criteria: File I/O (read), Branching (If file exists)
    ''' </summary>
    Private Sub LoadConfig()
        ' Branching: Check if config file exists
        If File.Exists(configFilePath) Then
            Try
                txtApiKey.Text = File.ReadAllText(configFilePath)
            Catch ex As Exception
                MessageBox.Show("Error loading config: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Procedure: UploadFile - Opens file dialog and auto-detects file type (image or text)
    ''' Criteria: File I/O (read with OpenFileDialog), Branching (file type detection), Looping (For Each line)
    ''' </summary>
    Private Sub UploadFile()
        Using openFileDialog As New OpenFileDialog()
            openFileDialog.Filter = "All supported files|*.txt;*.csv;*.log;*.md;*.json;*.xml;*.html;*.css;*.js;*.vb;*.cs;*.png;*.jpg;*.jpeg;*.gif;*.webp|Text files|*.txt;*.csv;*.log;*.md;*.json;*.xml;*.html;*.css;*.js;*.vb;*.cs|Image files|*.png;*.jpg;*.jpeg;*.gif;*.webp|All files|*.*"
            openFileDialog.Title = "Select a file to upload (text or image)"

            If openFileDialog.ShowDialog() = DialogResult.OK Then
                Try
                    Dim filePath = openFileDialog.FileName
                    Dim extension = Path.GetExtension(filePath).ToLower()
                    Dim fileName = Path.GetFileName(filePath)

                    ' Clear previous uploads
                    ClearUploadedContent()

                    ' Branching: Detect file type based on extension
                    If IsImageFile(extension) Then
                        ' Handle as image file
                        ProcessImageFile(filePath, fileName, extension)
                    ElseIf IsTextFile(extension) Then
                        ' Handle as text file
                        ProcessTextFile(filePath, fileName)
                    Else
                        ' Unknown extension - try to read as text
                        ProcessTextFile(filePath, fileName)
                    End If

                Catch ex As Exception
                    MessageBox.Show("Error reading file: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Function: IsImageFile - Checks if extension is an image type
    ''' Criteria: Branching (extension check), Looping (For Each extension)
    ''' </summary>
    Private Function IsImageFile(extension As String) As Boolean
        ' Looping: Check each image extension
        For Each imgExt In imageExtensions
            ' Branching: Compare extension
            If extension = imgExt Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Function: IsTextFile - Checks if extension is a text type
    ''' Criteria: Branching (extension check), Looping (For Each extension)
    ''' </summary>
    Private Function IsTextFile(extension As String) As Boolean
        ' Looping: Check each text extension
        For Each txtExt In textExtensions
            ' Branching: Compare extension
            If extension = txtExt Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Procedure: ProcessImageFile - Reads image and converts to base64
    ''' Criteria: File I/O (read bytes), Branching (MIME type detection)
    ''' </summary>
    Private Sub ProcessImageFile(filePath As String, fileName As String, extension As String)
        ' Read image file as bytes and convert to base64
        Dim imageBytes As Byte() = File.ReadAllBytes(filePath)
        uploadedImageBase64 = Convert.ToBase64String(imageBytes)
        uploadedImageName = fileName

        ' Branching: Determine MIME type based on extension
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

        lblAttachment.Text = "Image: " & fileName

        ' Show preview
        picPreview.Image = Image.FromFile(filePath)
        picPreview.Visible = True

        AppendPlainText("[System] Image uploaded: " & fileName, Color.Gray)
    End Sub

    ''' <summary>
    ''' Procedure: ProcessTextFile - Reads text file line by line
    ''' Criteria: File I/O (read lines), Looping (For Each line)
    ''' </summary>
    Private Sub ProcessTextFile(filePath As String, fileName As String)
        ' Read file and process lines using loop
        Dim lines As String() = File.ReadAllLines(filePath)
        Dim sb As New StringBuilder()

        ' Looping: For Each line in uploaded file
        For Each line As String In lines
            sb.AppendLine(line)
        Next

        uploadedFileContent = sb.ToString()
        uploadedFileName = fileName
        lblAttachment.Text = "Text: " & fileName

        ' Hide image preview for text files
        picPreview.Visible = False
        picPreview.Image = Nothing

        AppendPlainText("[System] Text file uploaded: " & fileName, Color.Gray)
    End Sub

    ''' <summary>
    ''' Helper: Clear all uploaded content
    ''' </summary>
    Private Sub ClearUploadedContent()
        uploadedFileContent = ""
        uploadedFileName = ""
        uploadedImageBase64 = ""
        uploadedImageName = ""
        uploadedImageMimeType = ""
        lblAttachment.Text = "Attachment: (none)"
        picPreview.Visible = False
        picPreview.Image = Nothing
    End Sub

    ''' <summary>
    ''' Procedure: ClearChat - Clears the chat display
    ''' </summary>
    Private Sub ClearChat()
        rtbChatDisplay.Clear()
        ClearUploadedContent()
    End Sub

    ''' <summary>
    ''' Procedure: SendMessage - Validates input and initiates API call
    ''' Criteria: Branching (If apiKey empty, If fileAttached, If imageAttached)
    ''' </summary>
    Private Async Sub SendMessage()
        Dim apiKey As String = txtApiKey.Text.Trim()
        Dim message As String = txtMessage.Text.Trim()

        ' Branching: Check if API key is provided
        If apiKey = "" Then
            MessageBox.Show("Please enter your API key first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Branching: Check if message is empty
        If message = "" Then
            MessageBox.Show("Please enter a message.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' Display user message with attachment info
        Dim displayMessage = "User: " & message

        ' Branching: Check what type of attachment is included
        If uploadedImageName <> "" Then
            displayMessage &= " [Image: " & uploadedImageName & "]"
        ElseIf uploadedFileName <> "" Then
            displayMessage &= " [File: " & uploadedFileName & "]"
        End If

        AppendPlainText(displayMessage, Color.Blue)

        ' Clear input
        txtMessage.Clear()

        ' Disable controls during API call
        SetControlsEnabled(False)

        ' Reset AI response buffer
        currentAiResponse.Clear()
        isFirstStreamChunk = True

        ' Call OpenRouter API
        Await CallOpenRouter(apiKey, message, uploadedFileContent, uploadedImageBase64, uploadedImageMimeType)

        ' Re-enable controls
        SetControlsEnabled(True)

        ' Clear uploaded content after sending
        ClearUploadedContent()
    End Sub

    ''' <summary>
    ''' Procedure: CallOpenRouter - Makes API request with multimodal support
    ''' Criteria: Looping (While reader.ReadLine for streaming), Branching (response status, image/text content)
    ''' </summary>
    Private Async Function CallOpenRouter(apiKey As String, prompt As String, fileContent As String, imageBase64 As String, imageMimeType As String) As Task
        Try
            ' Build the content array for multimodal support
            Dim contentParts As New List(Of Object)

            ' Add text content first (as recommended by docs)
            Dim textContent = prompt

            ' Branching: If file content attached, prepend to prompt
            If fileContent <> "" Then
                textContent = "[File Content]" & vbCrLf & fileContent & vbCrLf & vbCrLf & "[Message]" & vbCrLf & prompt
            End If

            contentParts.Add(New With {
                .type = "text",
                .text = textContent
            })

            ' Branching: If image attached, add to content array
            If imageBase64 <> "" Then
                Dim dataUrl = $"data:{imageMimeType};base64,{imageBase64}"
                contentParts.Add(New With {
                    .type = "image_url",
                    .image_url = New With {
                        .url = dataUrl
                    }
                })
            End If

            ' Prepare request body
            Dim requestBody = New With {
                .model = "google/gemini-3-pro-preview",
                .messages = New Object() {
                    New With {
                        .role = "user",
                        .content = contentParts.ToArray()
                    }
                },
                .stream = True
            }

            Dim jsonContent As String = JsonSerializer.Serialize(requestBody)
            Dim content As New StringContent(jsonContent, Encoding.UTF8, "application/json")

            ' Set up request
            Using request As New HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions")
                request.Headers.Add("Authorization", "Bearer " & apiKey)
                request.Content = content

                ' Send request with streaming
                Using response = Await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                    ' Branching: Check response status
                    If response.IsSuccessStatusCode Then
                        Using stream = Await response.Content.ReadAsStreamAsync()
                            Using reader As New StreamReader(stream)
                                ' Looping: While reading response lines (streaming)
                                Dim line As String = Await reader.ReadLineAsync()
                                While line IsNot Nothing
                                    ProcessStreamLine(line)
                                    line = Await reader.ReadLineAsync()
                                End While
                            End Using
                        End Using

                        ' Final scroll to end after streaming completes
                        Me.Invoke(Sub() ScrollToEnd())
                    Else
                        ' Branching: Show error for non-success status
                        Dim errorContent = Await response.Content.ReadAsStringAsync()
                        AppendPlainText("[Error] API returned status " & CInt(response.StatusCode) & ": " & errorContent, Color.Red)
                    End If
                End Using
            End Using

        Catch ex As Exception
            AppendPlainText("[Error] " & ex.Message, Color.Red)
        End Try
    End Function

    ''' <summary>
    ''' Helper: Process a single line from the streaming response
    ''' </summary>
    Private Sub ProcessStreamLine(line As String)
        If line.StartsWith("data: ") Then
            Dim jsonData = line.Substring(6).Trim()

            If jsonData = "[DONE]" Then
                ' Add newline at the end of response
                Me.Invoke(Sub() AppendFormattedText(vbCrLf, Color.Black, FontStyle.Regular))
                Return
            End If

            Try
                Using doc = JsonDocument.Parse(jsonData)
                    Dim root = doc.RootElement
                    Dim choices As JsonElement
                    If root.TryGetProperty("choices", choices) Then
                        If choices.GetArrayLength() > 0 Then
                            Dim firstChoice = choices(0)
                            Dim delta As JsonElement
                            If firstChoice.TryGetProperty("delta", delta) Then
                                Dim contentElement As JsonElement
                                If delta.TryGetProperty("content", contentElement) Then
                                    Dim text = contentElement.GetString()
                                    If text IsNot Nothing Then
                                        currentAiResponse.Append(text)
                                        ' Update UI progressively as each chunk arrives
                                        Me.Invoke(Sub() AppendStreamingText(text))
                                    End If
                                End If
                            End If
                        End If
                    End If
                End Using
            Catch
                ' Ignore parse errors for malformed chunks
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Helper: Append streaming text to display in real-time
    ''' </summary>
    Private Sub AppendStreamingText(text As String)
        ' Add "AI: " label before first chunk
        If isFirstStreamChunk Then
            AppendFormattedText("AI: ", Color.Green, FontStyle.Bold)
            isFirstStreamChunk = False
        End If

        ' Append the text chunk directly (without markdown parsing for speed)
        AppendFormattedText(text, Color.Black, FontStyle.Regular)
        ScrollToEnd()
    End Sub

    ''' <summary>
    ''' Procedure: RenderMarkdownResponse - Renders markdown text with formatting
    ''' Criteria: Looping (For Each line, regex matches), Branching (markdown syntax detection)
    ''' </summary>
    Private Sub RenderMarkdownResponse(markdownText As String)
        If rtbChatDisplay.InvokeRequired Then
            rtbChatDisplay.Invoke(Sub() RenderMarkdownInternal(markdownText))
        Else
            RenderMarkdownInternal(markdownText)
        End If
    End Sub

    Private Sub RenderMarkdownInternal(markdownText As String)
        ' Add "AI: " label
        AppendFormattedText("AI: ", Color.Green, FontStyle.Bold)

        Dim lines = markdownText.Split({vbCrLf, vbLf}, StringSplitOptions.None)
        Dim inCodeBlock As Boolean = False
        Dim codeBlockContent As New StringBuilder()
        Dim codeBlockLang As String = ""

        ' Looping: For Each line in markdown response
        For Each line As String In lines
            ' Branching: Check for code block start/end
            If line.StartsWith("```") Then
                If Not inCodeBlock Then
                    ' Start of code block
                    inCodeBlock = True
                    codeBlockLang = line.Substring(3).Trim()
                    codeBlockContent.Clear()
                Else
                    ' End of code block - render it
                    inCodeBlock = False
                    RenderCodeBlock(codeBlockContent.ToString(), codeBlockLang)
                End If
            ElseIf inCodeBlock Then
                ' Inside code block - accumulate
                codeBlockContent.AppendLine(line)
            Else
                ' Regular line - process inline markdown
                RenderMarkdownLine(line)
                AppendFormattedText(vbCrLf, Color.Black, FontStyle.Regular)
            End If
        Next

        ' Handle unclosed code block
        If inCodeBlock Then
            RenderCodeBlock(codeBlockContent.ToString(), codeBlockLang)
        End If

        AppendFormattedText(vbCrLf, Color.Black, FontStyle.Regular)
        ScrollToEnd()
    End Sub

    ''' <summary>
    ''' Helper: Render a single line with inline markdown formatting
    ''' </summary>
    Private Sub RenderMarkdownLine(line As String)
        ' Branching: Check for headers
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

        ' Branching: Check for bullet points
        Dim bulletMatch = Regex.Match(line, "^(\s*)([-*+]|\d+\.)\s+(.*)$")
        If bulletMatch.Success Then
            Dim indent = bulletMatch.Groups(1).Value
            Dim bullet = bulletMatch.Groups(2).Value
            Dim content = bulletMatch.Groups(3).Value
            AppendFormattedText(indent & "  " & bullet & " ", Color.DarkGray, FontStyle.Regular)
            RenderInlineMarkdown(content)
            Return
        End If

        ' Regular line - process inline formatting
        RenderInlineMarkdown(line)
    End Sub

    ''' <summary>
    ''' Helper: Process inline markdown (bold, italic, code, links)
    ''' Criteria: Looping (regex matches), Branching (format type detection)
    ''' </summary>
    Private Sub RenderInlineMarkdown(text As String)
        ' Pattern to match: **bold**, *italic*, `code`, [link](url)
        Dim pattern As String = "(\*\*(.+?)\*\*|\*(.+?)\*|`(.+?)`|\[(.+?)\]\((.+?)\))"
        Dim lastIndex As Integer = 0
        Dim matches = Regex.Matches(text, pattern)

        ' Looping: For Each regex match
        For Each match As Match In matches
            ' Add text before match
            If match.Index > lastIndex Then
                AppendFormattedText(text.Substring(lastIndex, match.Index - lastIndex), Color.Black, FontStyle.Regular)
            End If

            ' Branching: Determine format type
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

            lastIndex = match.Index + match.Length
        Next

        ' Add remaining text after last match
        If lastIndex < text.Length Then
            AppendFormattedText(text.Substring(lastIndex), Color.Black, FontStyle.Regular)
        End If
    End Sub

    ''' <summary>
    ''' Helper: Render a code block with syntax highlighting background
    ''' </summary>
    Private Sub RenderCodeBlock(code As String, language As String)
        ' Add language label if provided
        If language <> "" Then
            AppendFormattedText("[" & language & "]" & vbCrLf, Color.Gray, FontStyle.Italic)
        End If

        ' Render code with monospace styling and background
        Dim codeLines = code.TrimEnd().Split({vbCrLf, vbLf}, StringSplitOptions.None)
        For Each codeLine In codeLines
            AppendFormattedText("  " & codeLine & vbCrLf, Color.DarkGreen, FontStyle.Regular, Color.FromArgb(245, 245, 245))
        Next
    End Sub

    ''' <summary>
    ''' Helper: Append formatted text to RichTextBox
    ''' </summary>
    Private Sub AppendFormattedText(text As String, foreColor As Color, style As FontStyle, Optional backColor As Color = Nothing)
        If backColor = Nothing Then
            backColor = Color.White
        End If

        Dim startPos = rtbChatDisplay.TextLength
        rtbChatDisplay.AppendText(text)
        rtbChatDisplay.Select(startPos, text.Length)
        rtbChatDisplay.SelectionColor = foreColor
        rtbChatDisplay.SelectionBackColor = backColor
        rtbChatDisplay.SelectionFont = New Font(rtbChatDisplay.Font, style)
        rtbChatDisplay.Select(rtbChatDisplay.TextLength, 0)
    End Sub

    ''' <summary>
    ''' Helper: Append plain text with color (for system messages, errors, user input)
    ''' </summary>
    Private Sub AppendPlainText(text As String, foreColor As Color)
        If rtbChatDisplay.InvokeRequired Then
            rtbChatDisplay.Invoke(Sub() AppendPlainTextInternal(text, foreColor))
        Else
            AppendPlainTextInternal(text, foreColor)
        End If
    End Sub

    Private Sub AppendPlainTextInternal(text As String, foreColor As Color)
        AppendFormattedText(text & vbCrLf, foreColor, FontStyle.Regular)
        ScrollToEnd()
    End Sub

    ''' <summary>
    ''' Helper: Scroll to end of RichTextBox
    ''' </summary>
    Private Sub ScrollToEnd()
        rtbChatDisplay.SelectionStart = rtbChatDisplay.TextLength
        rtbChatDisplay.ScrollToCaret()
    End Sub

    ''' <summary>
    ''' Helper: Enable/disable controls during API call
    ''' </summary>
    Private Sub SetControlsEnabled(enabled As Boolean)
        btnSend.Enabled = enabled
        btnUpload.Enabled = enabled
        txtMessage.Enabled = enabled
        txtApiKey.Enabled = enabled
        btnSaveKey.Enabled = enabled
    End Sub

End Class

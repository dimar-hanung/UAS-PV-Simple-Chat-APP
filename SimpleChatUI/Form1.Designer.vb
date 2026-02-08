<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        lblApiKey = New Label()
        txtApiKey = New TextBox()
        btnSaveKey = New Button()
        rtbChatDisplay = New RichTextBox()
        btnClear = New Button()
        lblAttachment = New Label()
        btnUpload = New Button()
        picPreview = New PictureBox()
        txtMessage = New TextBox()
        btnSend = New Button()
        CType(picPreview, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lblApiKey
        ' 
        lblApiKey.AutoSize = True
        lblApiKey.Location = New Point(12, 15)
        lblApiKey.Name = "lblApiKey"
        lblApiKey.Size = New Size(51, 15)
        lblApiKey.TabIndex = 0
        lblApiKey.Text = "API Key:"
        ' 
        ' txtApiKey
        ' 
        txtApiKey.Location = New Point(69, 12)
        txtApiKey.Name = "txtApiKey"
        txtApiKey.PasswordChar = "*"c
        txtApiKey.Size = New Size(300, 23)
        txtApiKey.TabIndex = 1
        ' 
        ' btnSaveKey
        ' 
        btnSaveKey.Location = New Point(375, 11)
        btnSaveKey.Name = "btnSaveKey"
        btnSaveKey.Size = New Size(75, 25)
        btnSaveKey.TabIndex = 2
        btnSaveKey.Text = "Save"
        btnSaveKey.UseVisualStyleBackColor = True
        ' 
        ' rtbChatDisplay
        ' 
        rtbChatDisplay.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        rtbChatDisplay.BackColor = Color.White
        rtbChatDisplay.Font = New Font("Consolas", 10.0F, FontStyle.Regular, GraphicsUnit.Point)
        rtbChatDisplay.Location = New Point(12, 45)
        rtbChatDisplay.Name = "rtbChatDisplay"
        rtbChatDisplay.ReadOnly = True
        rtbChatDisplay.Size = New Size(560, 280)
        rtbChatDisplay.TabIndex = 3
        ' 
        ' btnClear
        ' 
        btnClear.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnClear.Location = New Point(497, 11)
        btnClear.Name = "btnClear"
        btnClear.Size = New Size(75, 25)
        btnClear.TabIndex = 9
        btnClear.Text = "Clear"
        btnClear.UseVisualStyleBackColor = True
        ' 
        ' lblAttachment
        ' 
        lblAttachment.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        lblAttachment.AutoSize = True
        lblAttachment.Location = New Point(98, 337)
        lblAttachment.Name = "lblAttachment"
        lblAttachment.Size = New Size(96, 15)
        lblAttachment.TabIndex = 4
        lblAttachment.Text = "Attachment: (none)"
        ' 
        ' btnUpload
        ' 
        btnUpload.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnUpload.Location = New Point(12, 332)
        btnUpload.Name = "btnUpload"
        btnUpload.Size = New Size(80, 25)
        btnUpload.TabIndex = 5
        btnUpload.Text = "Upload"
        btnUpload.UseVisualStyleBackColor = True
        ' 
        ' picPreview
        ' 
        picPreview.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        picPreview.BorderStyle = BorderStyle.FixedSingle
        picPreview.Location = New Point(12, 363)
        picPreview.Name = "picPreview"
        picPreview.Size = New Size(50, 50)
        picPreview.SizeMode = PictureBoxSizeMode.Zoom
        picPreview.TabIndex = 6
        picPreview.TabStop = False
        picPreview.Visible = False
        ' 
        ' txtMessage
        ' 
        txtMessage.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtMessage.Location = New Point(68, 380)
        txtMessage.Name = "txtMessage"
        txtMessage.Size = New Size(423, 23)
        txtMessage.TabIndex = 7
        ' 
        ' btnSend
        ' 
        btnSend.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnSend.Location = New Point(497, 379)
        btnSend.Name = "btnSend"
        btnSend.Size = New Size(75, 25)
        btnSend.TabIndex = 8
        btnSend.Text = "Send"
        btnSend.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AcceptButton = btnSend
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(584, 416)
        Controls.Add(btnClear)
        Controls.Add(btnSend)
        Controls.Add(txtMessage)
        Controls.Add(picPreview)
        Controls.Add(btnUpload)
        Controls.Add(lblAttachment)
        Controls.Add(rtbChatDisplay)
        Controls.Add(btnSaveKey)
        Controls.Add(txtApiKey)
        Controls.Add(lblApiKey)
        MinimumSize = New Size(500, 400)
        Name = "Form1"
        Text = "Simple LLM Chat"
        CType(picPreview, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblApiKey As Label
    Friend WithEvents txtApiKey As TextBox
    Friend WithEvents btnSaveKey As Button
    Friend WithEvents rtbChatDisplay As RichTextBox
    Friend WithEvents btnClear As Button
    Friend WithEvents lblAttachment As Label
    Friend WithEvents btnUpload As Button
    Friend WithEvents picPreview As PictureBox
    Friend WithEvents txtMessage As TextBox
    Friend WithEvents btnSend As Button

End Class

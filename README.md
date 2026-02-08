# SimpleChatUI

Aplikasi chat sederhana berbasis Windows Forms yang terintegrasi dengan AI melalui OpenRouter API.

## Deskripsi

SimpleChatUI adalah aplikasi desktop yang memungkinkan pengguna untuk berkomunikasi dengan model AI (Google Gemini) melalui antarmuka chat yang intuitif. Aplikasi ini mendukung pengiriman pesan teks, upload file teks, dan upload gambar untuk analisis multimodal.

## Fitur

- **Chat dengan AI**: Kirim pesan dan terima respons dari AI secara real-time dengan streaming
- **Upload File Teks**: Mendukung berbagai format file teks (.txt, .csv, .log, .md, .json, .xml, .html, .css, .js, .vb, .cs)
- **Upload Gambar**: Mendukung format gambar (.png, .jpg, .jpeg, .gif, .webp) untuk analisis visual
- **Penyimpanan API Key**: API key disimpan secara lokal untuk kemudahan penggunaan
- **Antarmuka Responsif**: Kontrol dinonaktifkan saat menunggu respons AI
- **Rendering Markdown**: Respons AI ditampilkan dengan format markdown (bold, italic, code block, dll)

## Persyaratan Sistem

- Windows 10/11
- .NET 8.0 Runtime
- Koneksi internet untuk mengakses API

## Instalasi

1. Clone repository ini
2. Buka file `UAS-PV.sln` dengan Visual Studio 2022
3. Build solusi (Ctrl + Shift + B)
4. Jalankan aplikasi (F5)

## Cara Penggunaan

1. **Masukkan API Key**: Dapatkan API key dari [OpenRouter](https://openrouter.ai/) dan masukkan di field "API Key"
2. **Simpan API Key**: Klik tombol "Save" untuk menyimpan API key (opsional, akan tersimpan otomatis saat aplikasi ditutup)
3. **Kirim Pesan**: Ketik pesan di text box dan klik "Send" atau tekan Enter
4. **Upload File** (Opsional): Klik "Upload" untuk melampirkan file teks atau gambar
5. **Hapus Chat**: Klik "Clear" untuk membersihkan riwayat chat

## Struktur Proyek

```
UAS-PV/
├── SimpleChatUI/
│   ├── Form1.vb              # Logic utama aplikasi
│   ├── Form1.Designer.vb     # Desain form (auto-generated)
│   ├── Form1.resx            # Resource file
│   ├── ApplicationEvents.vb  # Event handler aplikasi
│   ├── SimpleChatUI.vbproj   # Project file
│   └── My Project/           # Konfigurasi proyek VB.NET
├── UAS-PV.sln                # Solution file
└── README.md                 # Dokumentasi
```

## Teknologi yang Digunakan

- **Bahasa**: Visual Basic .NET (VB.NET)
- **Framework**: .NET 8.0
- **UI Framework**: Windows Forms
- **API**: OpenRouter API (model: Google Gemini 3 Pro)
- **IDE**: Visual Studio 2022

## Kriteria Pemrograman

Aplikasi ini mengimplementasikan beberapa kriteria pemrograman dasar:

| Kriteria | Implementasi |
|----------|--------------|
| File I/O | Baca/tulis config.txt untuk menyimpan API key, baca file upload |
| Branching (If) | Validasi input, deteksi tipe file, pengecekan status API |
| Looping (For Each) | Iterasi ekstensi file, parsing respons streaming, rendering markdown |

## Catatan Keamanan

- API key disimpan dalam file `config.txt` di folder aplikasi
- Jangan membagikan API key Anda kepada orang lain
- Pastikan untuk menambahkan `config.txt` ke `.gitignore` jika menggunakan version control

## Lisensi

Proyek ini dibuat untuk keperluan UAS Pemrograman Visual.

## Kontributor

- [Nama Anda]

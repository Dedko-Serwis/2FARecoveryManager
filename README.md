# 2FARecoveryManager

[Polski](README.pl.md) | English

A Windows desktop utility for creating **TOTP authenticator enrollment QR codes** from an existing Base32 secret or a text phrase, and displaying the corresponding six-digit one-time code.

## Features
- WPF desktop interface (.NET 10, Windows)
- Issuer and account labels for authenticator setup
- Two input modes: **Recovery Key (Base32)** (existing TOTP secret) and **Text Phrase** (UTF-8 text converted to Base32)
- QR code generation using the `otpauth://totp/` format
- Live six-digit TOTP (SHA-1, 30-second period)
- Copy the generated Base32 secret to the clipboard

## Important limitations
**This app does not recover lost 2FA accounts or convert service-issued backup/recovery codes into the original TOTP secret.** Text Phrase mode creates a TOTP secret from text; it does not retrieve an existing secret. Use it only when the target service supports registering that exact secret. Do not use a memorable or low-entropy phrase as a security-critical secret.

The QR code contains the secret. Anyone who scans or photographs it can generate the same authentication codes. Avoid screenshots, screen sharing and copying secrets unnecessarily. Clipboard contents can remain accessible after copying.

## Requirements and build
- Windows with .NET 10 Desktop Runtime (for framework-dependent builds)
- .NET 10 SDK for building

```powershell
git clone https://github.com/Dedko-Serwis/2FARecoveryManager.git
cd 2FARecoveryManager
dotnet restore
dotnet build -c Release
dotnet run --project 2FARecoveryManager.csproj
```

## Usage
1. Enter the issuer and account name.
2. Choose **Recovery Key (Base32)** for an existing authenticator secret, or **Text Phrase** to encode text as a new secret.
3. Enter the value; the QR code and current TOTP are generated automatically.
4. Scan the QR code with a compatible authenticator. Verify that both apps show the same code.

The tool generates a QR code locally. It does not register the secret with an online service.

## Dependencies and licensing
- [QRCoder](https://github.com/codebude/QRCoder) 1.8.0 — MIT license.
- .NET / WPF — Microsoft platform components, subject to their respective licensing terms.

This project's source code is released under the [MIT License](LICENSE). See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Security note
The current implementation keeps the entered secret in application memory, displays the secret in the interface and can copy it to the clipboard. It is **not** a secure credential vault. Review the source and your threat model before handling real authentication secrets.

Copyright (c) 2026 Leszek Dedko.

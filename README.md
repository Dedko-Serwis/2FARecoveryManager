# 2FARecoveryManager

[Polski](README.pl.md) | English

## What is this application for?

**2FARecoveryManager helps restore a TOTP authenticator entry on a replacement phone after the previous phone is lost, damaged or replaced.** When enabling two-factor authentication (2FA), many services display a QR code and may also provide a secret key. If you saved that **original TOTP secret**, this application can recreate a compatible QR code so you can scan it with an authenticator app again—without needing the old phone.

The program cannot extract a secret from a lost phone or from a service's one-time backup/recovery codes. It works only with a secret you already have.

## How to restore an authenticator entry

1. Enter the service name (**Issuer**), e.g. GitHub, and an account label, e.g. your email address. These identify the entry in the authenticator.
2. Select **TOTP Key (Base32)** and enter the **original saved secret** from the service.
3. Scan the generated QR code with a compatible authenticator on the new phone.
4. Compare the six-digit code displayed by the application with the code shown on the phone. Both should match when their clocks are synchronized and the service uses standard SHA-1 / 6 digits / 30 seconds.

**Important:** Scanning the QR code restores the same TOTP codes only when the input is the same original secret and the TOTP parameters match those used by the service. It does not change or register 2FA settings on the service.

## Input fields

- **Service / Issuer:** A descriptive name, e.g. GitHub.
- **Account name:** The account identifier, e.g. an email address.
- **TOTP Key (Base32):** The existing secret that the service originally encoded in its setup QR code. This is the recommended recovery mode.
- **Text phrase (UTF-8):** Converts the entered text bytes to Base32 to create a TOTP secret. This is **not a general recovery method**: it reproduces existing codes only if the service was originally configured with precisely those bytes as its secret.
- **Saved phrase or key:** The sensitive input for the selected mode.

A service's **backup/recovery codes** are different from the TOTP secret and cannot be used here to reconstruct its QR code.

## Features

- Local QR code generation using the `otpauth://totp/` format
- Live six-digit TOTP codes (SHA-1, 30-second period)
- Base32 and UTF-8 phrase input modes
- Optional copying of the resulting Base32 secret to the clipboard, with an attempt to clear it after 30 seconds if unchanged
- Windows desktop interface built with WPF and .NET 10

## Security and limitations

The QR code and original secret grant access to the same TOTP codes. Protect them like passwords. Avoid screenshots and screen sharing. The program does not store a password-protected account vault and retains secrets in application memory while in use. Its current QR/TOTP implementation uses SHA-1, six digits and a 30-second interval; it does not import arbitrary `otpauth://` URIs or other TOTP algorithms.

## Requirements and build

Windows and .NET 10 Desktop Runtime are required for the framework-dependent application. To build from source, install the .NET 10 SDK.

```powershell
git clone https://github.com/Dedko-Serwis/2FARecoveryManager.git
cd 2FARecoveryManager
dotnet build -c Release
dotnet run --project 2FARecoveryManager.csproj
```

To create a release installer, install Inno Setup 6 or 7 and run `./scripts/build-release.ps1`. The script reads the version from `2FARecoveryManager.csproj`, runs tests, publishes a framework-dependent win-x64 build, and creates the installer in `dist/`. The installer supports Polish and English based on Windows UI language.

## Licensing

Project source code: [MIT License](LICENSE). Direct dependency: [QRCoder](https://github.com/codebude/QRCoder) 1.8.0 (MIT). .NET/WPF are Microsoft platform components subject to their own terms. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

Copyright (c) 2026 Leszek Dedko.

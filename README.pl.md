# 2FARecoveryManager

Polski | [English](README.md)

Program dla Windows do generowania **kodów QR konfiguracji TOTP** na podstawie istniejącego klucza Base32 lub frazy tekstowej oraz do wyświetlania odpowiadającego mu sześciocyfrowego kodu jednorazowego.

## Funkcje
- Aplikacja WPF (.NET 10, Windows)
- Pola wystawcy (Issuer) i konta (Account)
- Dwa tryby: **Recovery Key (Base32)** — istniejący sekret TOTP; **Text Phrase** — tekst UTF-8 kodowany do Base32
- Generowanie QR w formacie `otpauth://totp/`
- Aktualny sześciocyfrowy kod TOTP (SHA-1, okres 30 sekund)
- Kopiowanie sekretu Base32 do schowka

## Ważne ograniczenia
**Program nie odzyskuje utraconego dostępu do kont 2FA i nie zamienia zapasowych kodów odzyskiwania wydanych przez usługę w pierwotny sekret TOTP.** Tryb Text Phrase tworzy sekret z tekstu, ale nie odtwarza istniejącego sekretu. Ma sens tylko wtedy, gdy dana usługa umożliwia zarejestrowanie dokładnie tego sekretu. Nie używaj łatwej do odgadnięcia frazy jako klucza chroniącego konto.

Kod QR zawiera sekret. Każdy, kto go zeskanuje lub sfotografuje, może generować te same kody. Unikaj zrzutów ekranu i udostępniania obrazu. Sekret skopiowany do schowka może tam pozostać.

## Wymagania i kompilacja
- Windows i .NET 10 Desktop Runtime (przy publikacji zależnej od środowiska)
- .NET 10 SDK do kompilacji

```powershell
git clone https://github.com/Dedko-Serwis/2FARecoveryManager.git
cd 2FARecoveryManager
dotnet restore
dotnet build -c Release
dotnet run --project 2FARecoveryManager.csproj
```

## Obsługa
1. Wpisz nazwę wystawcy i konta.
2. Wybierz **Recovery Key (Base32)** dla posiadanego sekretu uwierzytelniania albo **Text Phrase**, aby zakodować tekst jako nowy sekret.
3. Wpisz wartość — program automatycznie wyświetli kod QR i aktualny TOTP.
4. Zeskanuj QR zgodnym programem Authenticator i porównaj wyświetlane kody.

Program generuje QR lokalnie; nie rejestruje sekretu w usłudze internetowej.

## Biblioteki i licencja
- [QRCoder](https://github.com/codebude/QRCoder) 1.8.0 — licencja MIT.
- .NET / WPF — komponenty platformy Microsoft, objęte odpowiednimi warunkami licencyjnymi.

Kod źródłowy projektu jest udostępniony na [licencji MIT](LICENSE). Informacje o zależnościach: [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Bezpieczeństwo
Obecna implementacja przechowuje wpisany sekret w pamięci aplikacji, pokazuje go w interfejsie i umożliwia kopiowanie do schowka. **Nie jest to bezpieczny menedżer haseł ani sejf na sekrety.**

Copyright (c) 2026 Leszek Dedko.

## Instalator Release
Na Windows zainstaluj .NET 10 SDK i Inno Setup 7, a następnie uruchom `./scripts/build-release.ps1` w PowerShell. Skrypt uruchamia testy, publikuje wersję win-x64 zależną od środowiska .NET i buduje instalator w `dist/`. Język instalatora jest dobierany do języka Windows (polski lub angielski). .NET 10 Desktop Runtime należy zainstalować osobno.

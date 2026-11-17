# 2FARecoveryManager

Polski | [English](README.md)

## Do czego służy program?

**2FARecoveryManager umożliwia odtworzenie konfiguracji uwierzytelniania dwuskładnikowego (2FA/TOTP) na nowym telefonie po zgubieniu, uszkodzeniu lub wymianie poprzedniego.** Podczas włączania 2FA wiele serwisów pokazuje kod QR, a często także klucz tekstowy. Jeśli wcześniej zapisano **oryginalny sekret TOTP**, program może ponownie wygenerować kod QR, który wystarczy zeskanować aplikacją Authenticator na nowym telefonie.

Nie jest potrzebny stary telefon. Potrzebny jest natomiast **zachowany wcześniej sekret** — program nie odczytuje go z uszkodzonego urządzenia ani nie odtwarza z jednorazowych kodów zapasowych serwisu.

## Jak odzyskać generator kodów 2FA?

1. W polu **Serwis / wystawca** wpisz nazwę usługi, np. GitHub.
2. W polu **Nazwa konta** wpisz identyfikator konta, np. adres e-mail. Te dwa pola służą do opisania wpisu w aplikacji Authenticator.
3. Wybierz **Klucz TOTP (Base32)** i wprowadź **oryginalny sekret** zapisany podczas konfiguracji 2FA.
4. Zeskanuj wygenerowany kod QR aplikacją Authenticator na nowym telefonie.
5. Porównaj sześciocyfrowy kod na telefonie z kodem w programie. Powinny być identyczne, jeśli zegary są zsynchronizowane, a serwis używa standardowych parametrów SHA-1 / 6 cyfr / 30 sekund.

**Ważne:** Odtworzenie tych samych kodów jest możliwe tylko wtedy, gdy podany sekret jest identyczny z oryginalnym, a parametry TOTP są zgodne z ustawieniami serwisu. Program nie zmienia konfiguracji 2FA po stronie serwisu.

## Opis pól

- **Serwis / wystawca** — nazwa usługi, np. GitHub.
- **Nazwa konta** — identyfikator użytkownika, np. adres e-mail.
- **Klucz TOTP (Base32)** — istniejący sekret zapisany podczas włączania 2FA. **To zalecany tryb odtwarzania.**
- **Fraza tekstowa (UTF-8)** — zamienia bajty wpisanego tekstu na sekret Base32. **Nie jest to uniwersalna metoda odzyskiwania**: odtworzy wcześniejsze kody tylko wtedy, gdy serwis od początku korzystał z dokładnie tych samych bajtów jako sekretu.
- **Zapisana fraza lub klucz** — pole na poufne dane wybranego formatu.

**Kody zapasowe / recovery codes** wydawane przez serwis nie są sekretem TOTP i nie pozwalają odtworzyć kodu QR za pomocą tego programu.

## Funkcje

- Lokalne generowanie kodu QR zgodnego z `otpauth://totp/`
- Wyświetlanie aktualnego sześciocyfrowego kodu TOTP (SHA-1, 30 sekund)
- Obsługa sekretu Base32 i frazy UTF-8
- Kopiowanie sekretu Base32 do schowka, z próbą usunięcia po 30 sekundach, jeśli nie zmieniono jego zawartości
- Aplikacja Windows WPF oparta na .NET 10

## Bezpieczeństwo i ograniczenia

Kod QR zawiera sekret umożliwiający generowanie kodów uwierzytelniających. Chroń go jak hasło; unikaj zrzutów ekranu i udostępniania obrazu. Program nie jest szyfrowanym magazynem kont. Podczas działania sekret znajduje się w pamięci aplikacji. Obecna wersja używa parametrów SHA-1, sześciu cyfr i okresu 30 sekund; nie importuje dowolnych adresów `otpauth://` ani innych algorytmów TOTP.

## Wymagania i kompilacja

Aplikacja wymaga Windows oraz .NET 10 Desktop Runtime (wersja zależna od środowiska). Do kompilacji potrzebny jest .NET 10 SDK.

```powershell
git clone https://github.com/Dedko-Serwis/2FARecoveryManager.git
cd 2FARecoveryManager
dotnet build -c Release
dotnet run --project 2FARecoveryManager.csproj
```

Aby utworzyć instalator, zainstaluj Inno Setup 6 lub 7 i uruchom `./scripts/build-release.ps1`. Skrypt pobiera wersję z `2FARecoveryManager.csproj`, uruchamia testy, publikuje aplikację win-x64 zależną od .NET i tworzy instalator w katalogu `dist/`. Instalator obsługuje język polski i angielski zgodnie z językiem interfejsu Windows.

## Licencja

Kod źródłowy: [MIT](LICENSE). Biblioteka: [QRCoder](https://github.com/codebude/QRCoder) 1.8.0 (MIT). .NET/WPF są komponentami Microsoft na odpowiednich warunkach licencyjnych. Szczegóły: [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

Copyright (c) 2026 Leszek Dedko.

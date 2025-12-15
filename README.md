# Watcher

[![Test Coverage](https://codecov.io/gh/Devscord-Team/Watcher/graph/badge.svg?token=71BI6043KV)](https://codecov.io/gh/Devscord-Team/Watcher)

Bot discordowy. Reaktywacja projektu [Watchman](https://github.com/Devscord-Team/Watchman), ale pisana od zera.

## Kontekst

Watchman powstał jesienią 2019 jako bot do moderacji. Główne funkcjonalności to:

- **Wykrywanie spamu** — wielopoziomowy system z detektorami (flood, duplikaty, capslock, linki) i klasyfikacją użytkowników (zaufany/neutralny/podejrzany). Użytkownicy z historią problemów byli traktowani bardziej rygorystycznie, aktywni członkowie serwera mieli większą tolerancję.
- **Wyciszanie użytkowników** — zarówno automatyczne (przy wykryciu spamu) jak i ręczne przez adminów, z progresywnymi czasami kar.
- **Statystyki** — generowanie wykresów aktywności serwera z różną granulacją (godzina/dzień/tydzień/miesiąc).

Przy okazji zbudowaliśmy sporo rozwiązań wewnętrznych: własny framework do obsługi Discorda (izolacja od Discord.NET), własną implementację CQRS, parser komend, system konfigurowalnych odpowiedzi, generator helpa, mechanizm "bezpiecznych ról" które użytkownicy mogli sobie sami przypisywać.

Projekt był aktywnie rozwijany przez prawie cały 2020 rok. Przez repo przewinęło się 18 osób, powstało ponad 2000 commitów.

Pod koniec 2020 aktywność zaczęła spadać, a Discord w międzyczasie dodawał coraz więcej funkcji moderacyjnych natywnie (AutoMod, lepsze zarządzanie rolami, wbudowane timeouty). Projekt umarł śmiercią naturalną.

W 2022 była krótka próba reaktywacji, ale polegała na przywróceniu do życia starego kodu. Problem w tym, że w 2022 natywne funkcje Discorda pokrywały jeszcze więcej niż wcześniej — sens istnienia bota moderacyjnego był jeszcze mniejszy. Nic z tego nie wyszło.

Watcher to nowe podejście. Ten sam bot (z perspektywy Discorda — ta sama aplikacja, ten sam token), ale zupełnie inny cel i kod. Zero linijek ze starego repo.

## Pomysł

Zamiast moderacji — analiza. Bot który "rozumie" co się dzieje na serwerze:

**Klasyfikacja tematów** — automatyczne rozpoznawanie o czym jest rozmowa, śledzenie kiedy temat się zmienia, statystyki popularności tematów w czasie.

**Graf relacji** — kto z kim rozmawia, jakie są "grupy" na serwerze, wizualizacja powiązań między użytkownikami.

**Profile zainteresowań** — przy jakich tematach kto się udziela, kto jest "ekspertem" od czego (na podstawie aktywności, nie deklaracji).

**Powiadomienia o aktywności** — zamiast wyboru między "powiadom o każdej wiadomości" a "powiadom tylko o @mention", opcja w stylu "daj znać jak się coś dzieje". Wykrywanie skoków aktywności, interesujących dyskusji, tematów które mogą mnie obchodzić.

To wstępny plan. Może się zmienić, może coś wyrzucimy, może dodamy coś zupełnie innego.

## Podejście

Główny cel to zabawa i eksperymenty. Jeśli coś będzie ciekawe do zbudowania (własny pipeline ML, event sourcing, nietypowa architektura, własna baza danych), to to zbudujemy — nawet jeśli istnieje prostsze rozwiązanie. Overengineering jest częścią zabawy.

## Stack

- .NET 9

Reszta jeszcze nie wybrana.

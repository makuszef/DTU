<!-- README.md for Hash Function Benchmarking Project -->

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-blue" alt=".NET 8.0" />
  <img src="https://img.shields.io/badge/C%23-Project-brightgreen" alt="C# Project" />
  <img src="https://img.shields.io/badge/License-MIT-yellow" alt="License: MIT" />
</p>

# 🔒 Hash Function Performance Benchmark

**Badanie wydajności funkcji skrótu** w C# (.NET 8.0), porównujące:
- 🔹 MD5  
- 🔹 SHA-1  
- 🔹 SHA3-512  
- 🔹 Whirlpool-512  

Projekt automatyzuje pomiar latencji, przepustowości oraz zużycia CPU/RAM na dowolnych plikach.

---

## 📋 Spis treści

1. [Funkcje skrótu](#funkcje-skrótu)  
2. [Przygotowanie środowiska](#przygotowanie-środowiska)  
3. [Struktura projektu](#struktura-projektu)  
4. [Uruchomienie benchmarku](#uruchomienie-benchmarku)  
5. [Wyniki](#wyniki)  
6. [Licencja](#licencja)  

---

## 🔍 Funkcje skrótu

| Algorytm    | Długość skrótu | Uwagi                                 |
|-------------|----------------|---------------------------------------|
| **MD5**     | 128 bitów      | najszybszy, ale niebezpieczny         |
| **SHA-1**   | 160 bitów      | szybki, ale podatny na kolizje        |
| **SHA3-512**| 512 bitów      | wysoka odporność, średnia szybkość    |
| **Whirlpool**| 512 bitów     | bardzo bezpieczny, nieco wolniejszy   |

---

## ⚙️ Przygotowanie środowiska

1. **.NET 8.0 SDK**  
   Pobierz i zainstaluj z:  
   https://dotnet.microsoft.com/download/dotnet/8.0

2. **BouncyCastle** (NuGet)  
   ```bash
   dotnet add package BouncyCastle
   ```

3. **Katalog testowy**  
   Utwórz folder `TestowanePliki` obok skompilowanego EXE i umieść tam pliki do testów.

---

## 📁 Struktura projektu

```
/BenchmarkHash
  ├─ Program.cs              # Główny kod benchmarku
  ├─ BenchmarkResult.cs      # Model podsumowania
  ├─ IterationResult.cs      # Model pojedynczej iteracji
  ├─ TestowanePliki/         # Folder z plikami testowymi
  ├─ <NazwaPlikuBezExt>/     # Foldery wyników (tworzone automatycznie)
  │   ├─ md5.csv
  │   ├─ sha1.csv
  │   ├─ sha3_512.csv
  │   ├─ whirlpool.csv
  │   └─ hash_benchmark_results.csv
  └─ README.md
```

---

## ▶️ Uruchomienie benchmarku

1. **Klonuj repozytorium**  
   ```bash
   git clone https://github.com/TwojRepo/BenchmarkHash.git
   cd BenchmarkHash
   ```

2. **Zbuduj projekt**  
   ```bash
   dotnet build -c Release
   ```

3. **Przygotuj pliki**  
   Umieść pliki do testów w `bin/Release/net8.0/TestowanePliki/`.

4. **Uruchom**  
   ```bash
   dotnet bin/Release/net8.0/BenchmarkHash.dll
   ```

5. **Sprawdź wyniki**  
   Dla każdego testowanego pliku zostanie utworzony folder `<NazwaPlikuBezExt>` zawierający:
   - `md5.csv`, `sha1.csv`, `sha3_512.csv`, `whirlpool.csv` – surowe czasy każdej iteracji  
   - `hash_benchmark_results.csv` – podsumowanie statystyczne  

---

## 📊 Wyniki

- **LatencyMs** – czas każdej iteracji (ms)  
- **CpuTimeSec** – czas CPU wykorzystany w iteracji (s)  
- **MemoryUsageMB** – przyrost pamięci (MB)  
- **ThroughputMBps** – przepustowość (MB/s)  
- **Avg, StdDev, P50, P95, P99** – statystyki latencji  
- Całkowite zużycie CPU i pamięci dla zestawu 10 iteracji  

---

## 📝 Licencja

Projekt udostępniony na licencji [MIT](LICENSE).  

---

<p align="center">Made with ❤️ at WAT CYBERNETYKA</p>

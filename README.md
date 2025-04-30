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

| Algorytm     | Długość skrótu | Uwagi                                 |
|--------------|----------------|---------------------------------------|
| **MD5**      | 128 bitów      | najszybszy, ale niebezpieczny         |
| **SHA-1**    | 160 bitów      | szybki, ale podatny na kolizje        |
| **SHA3-512** | 512 bitów      | wysoka odporność, średnia szybkość    |
| **Whirlpool**| 512 bitów      | bardzo bezpieczny, nieco wolniejszy   |

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
   Utwórz folder `TestowanePliki` w ścieżce:  
   ```
   Kod/DTU/bin/Debug/net8.0/TestowanePliki
   ```
   i umieść tam pliki do testów.

---

## 📁 Struktura projektu

```
Kod/DTU
├─ bin/Debug/net8.0/
│  ├─ DTU.dll                      # skompilowana aplikacja
│  ├─ TestowanePliki/              # pliki do testów
│  ├─ <NazwaPlikuBezExt>/          # folder wyników tworzony automatycznie
│  │   ├─ md5.csv
│  │   ├─ sha1.csv
│  │   ├─ sha3_512.csv
│  │   ├─ whirlpool.csv
│  │   └─ hash_benchmark_results.csv
│  └─ README.md
├─ Program.cs
├─ BenchmarkResult.cs
├─ IterationResult.cs
└─ README.md
```

---

## ▶️ Uruchomienie benchmarku

1. **Klonuj repozytorium**  
   ```bash
   git clone https://github.com/makuszef/DTU.git
   cd DTU
   ```

2. **Zbuduj projekt**  
   ```bash
   dotnet build -c Debug
   ```

3. **Przygotuj pliki**  
   Umieść pliki do testów w:  
   ```
   Kod/DTU/bin/Debug/net8.0/TestowanePliki/
   ```

4. **Uruchom**  
   ```bash
   cd Kod/DTU/bin/Debug/net8.0
   dotnet DTU.dll
   ```

5. **Sprawdź wyniki**  
   Dla każdego testowanego pliku zostanie utworzony folder `<NazwaPlikuBezExt>` w `bin/Debug/net8.0`, zawierający:
   - `md5.csv`, `sha1.csv`, `sha3_512.csv`, `whirlpool.csv` – surowe czasy każdej iteracji  
   - `hash_benchmark_results.csv` – podsumowanie statystyczne  

---

## 📊 Wyniki

- **LatencyMs** – czas każdej iteracji (ms)  
- **CpuTimeSec** – czas CPU wykorzystany w iteracji (s)  
- **MemoryUsageMB** – przyrost pamięci (MB)  
- **ThroughputMBps** – przepustowość (MB/s)  
- **Avg, StdDev, P50, P95, P99** – statystyki latencji  
- **CpuUsageDiffSec**, **MemoryUsageDiffMB** – całkowite zużycie zasobów

---

## 📝 Licencja

Projekt udostępniony na licencji [MIT](LICENSE).  

---

<p align="center">Made with ❤️ at WAT CYBERNETYKA</p>

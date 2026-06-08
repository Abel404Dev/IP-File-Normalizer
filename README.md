# 🚀 IP File Normalizer

![Banner](assets/banner.png)

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D4?logo=windows)]()
[![License](https://img.shields.io/badge/License-MIT-green)]()
[![Version](https://img.shields.io/badge/Version-1.0.1-orange)]()

[![GitHub Stars](https://img.shields.io/github/stars/HosseinEP-Dev/test?style=for-the-badge)]()
[![GitHub Forks](https://img.shields.io/github/forks/HosseinEP-Dev/test?style=for-the-badge)]()
[![GitHub Issues](https://img.shields.io/github/issues/HosseinEP-Dev/test?style=for-the-badge)]()
[![GitHub Downloads](https://img.shields.io/github/downloads/HosseinEP-Dev/test/total?style=for-the-badge)]()

---

### ⚡ Fast • Lightweight • Memory Efficient • Multi-File IP Processing

A high-performance .NET utility for cleaning, normalizing, merging, deduplicating and sorting massive IPv4 datasets.

---

## 📑 Table of Contents

- [✨ Features](#-features)
- [📖 Overview](#-overview)
- [📈 Statistics Engine](#-statistics-engine)
- [📂 Output Structure](#-output-structure)
- [⚡ Performance](#-performance)
- [🚀 Quick Start](#-quick-start)
- [📋 Usage](#-usage)
- [📸 Examples](#-examples)
- [🎯 Use Cases](#-use-cases)
- [🛠 Requirements](#-requirements)
- [🤝 Contributing](#-contributing)
- [📜 License](#-license)

---

## 📖 Overview

IP File Normalizer is a high-speed console application designed to process large text files containing IPv4 addresses.

### 🎯 Core Capabilities

- 🔗 Merge multiple files
- 🧹 Remove empty lines
- 🔁 Remove duplicates
- 🎯 Remove invalid IPs
- 📝 Strip unwanted text
- 🔌 Remove ports
- 📊 Sort IPv4 addresses
- 📁 Generate categorized result files

---

## ✨ Features

| Feature | Description |
|----------|------------|
| 🧹 NullFix | Remove empty lines |
| 🔁 DeDup | Remove duplicate lines and IPs |
| 🎯 InFix | Remove invalid IPv4 addresses |
| 📝 LetOut | Remove unwanted text around IPs |
| 🔌 DePort | Remove ports from IP:PORT entries |
| 📈 SortUp | Ascending IPv4 sort |
| 📉 SortDown | Descending IPv4 sort |
| 🔥 Del4 | Remove all IPv4 addresses |
| 🔗 Merger | Merge multiple files |
| 🚀 ElonMod | Full automatic cleanup pipeline |

---

## 🚀 ElonMod Pipeline

```text
NullFix
   ↓
DeDup
   ↓
InFix
   ↓
LetOut
   ↓
DePort
   ↓
SortUp
```

One click. Fully cleaned dataset.

---

## 📈 Statistics Engine

Before any operation starts, the application automatically analyzes the dataset.

### Collected Metrics

- Total Lines
- Empty Lines
- Duplicate Lines
- Lines With Text
- Lines With Ports
- Used Ports
- Total IPs
- Invalid IPs
- Duplicate IPs
- Valid IPv4 Count

### Example

```text
Total Lines       : 1,000,000
Empty Lines       : 1,204
Duplicate Lines   : 15,482
Invalid IPs       : 8,941
Valid IPv4s       : 974,373
```

---

## 📂 Output Structure

```text
Results
│
├── 1. ElonMod
├── 2. Merger
├── 3. MultiMod
│
├── NullFix
├── DeDup
├── InFix
├── LetOut
├── DePort
├── SortUp
├── SortDown
└── Del4
```

Generated filenames:

```text
06-15-2025_12-45-11_ElonMod.txt
```

---

## ⚡ Performance

Built for large-scale datasets.

### Technologies

- ConcurrentDictionary
- Parallel Processing
- Async File Operations
- Memory-Based Processing

### Optimized For

✅ Hundreds of thousands of lines

✅ Millions of IPv4 addresses

✅ Multi-file processing

✅ High-speed cleanup operations

---

## 🚀 Quick Start

### Clone

```bash
git clone https://github.com/USERNAME/IP-File-Normalizer.git
```

### Enter Directory

```bash
cd IP-File-Normalizer
```

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run
```

---

## 📋 Usage

Input file:

```text
C:\IPs\file1.txt
```

Multiple files:

```text
C:\IPs\file1.txt,C:\IPs\file2.txt,C:\IPs\file3.txt
```

Available operations:

```text
1 = NullFix
2 = DeDup
3 = InFix
4 = LetOut
5 = DePort
6 = SortUp
7 = SortDown
8 = Del4
9 = Merger
0 = ElonMod
```

---

## 📸 Examples

### Input

```text
ServerA:192.168.1.1:80

192.168.1.1
999.999.999.999

Google DNS 8.8.8.8:53
```

### Output (ElonMod)

```text
8.8.8.8
192.168.1.1
```

---

## 🎯 Use Cases

- 🌐 Proxy List Cleanup
- 🔎 OSINT Datasets
- 🛡 Firewall Imports
- 📡 Network Inventory Management
- 🔬 Security Research
- 🚨 Vulnerability Scanning Preparation
- 📊 Large IPv4 Data Processing

---

## 🛠 Requirements

- .NET 9 SDK

Download:

https://dotnet.microsoft.com/download

---

## 🤝 Contributing

Contributions are welcome.

If you have ideas, bug reports or improvements:

- Open an Issue
- Submit a Pull Request
- Suggest New Features

---

## 📜 License

MIT License

---

Made with ❤️ and ☕ for the networking & security community.

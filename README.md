# 🚀 IP File Normalizer

![Banner](assets/banner.png)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Self-Contained](https://img.shields.io/badge/Self--Contained-Yes-success)]()
[![Version](https://img.shields.io/badge/Version-2.0.0-orange)]()
[![License](https://img.shields.io/badge/License-MIT-green)]()

[![Windows](https://img.shields.io/badge/Windows-Supported-0078D6?logo=windows)]()
[![Linux](https://img.shields.io/badge/Linux-Supported-FCC624?logo=linux\&logoColor=black)]()
[![macOS](https://img.shields.io/badge/macOS-Supported-000000?logo=apple)]()

---

### ⚡ Fast • Lightweight • Memory Efficient
### Multi-File IP Processing • Cross-Platform • Self-Contained

A high-performance .NET 10 utility for cleaning, normalizing, merging, deduplicating and sorting massive IPv4 datasets.

**IP File Normalizer** is distributed as a **Self-Contained Application**, allowing end users to run it without installing the .NET SDK or .NET Runtime.

---

## 📑 Table of Contents

- [✨ Features](#-features)

- [📖 Overview](#-overview)

- [🏗 Architecture](#-architecture)

- [📁 Supported Input Formats](#-supported-input-formats)

- [💻 User Interface](#-user-interface)

- [📈 Statistics Engine](#-statistics-engine)

- [📂 Output Structure](#-output-structure)

- [⚡ Performance](#-performance)

- [🚀 Quick Start](#-quick-start)

- [📋 Usage](#-usage)

- [⚠️ Operation Priority Rules](#-operation-priority-rules)

- [📸 Examples](#-examples)

- [🎯 Use Cases](#-use-cases)

- [🛠 Requirements](#-requirements)

- [🤝 Contributing](#-contributing)

- [📜 License](#-license)

---

## 📖 Overview
IP File Normalizer is a high-speed cross-platform command-line application designed to process large text files containing IPv4 addresses.

### 🎯 Core Capabilities

- 🔗 Merge multiple TXT files

- 🧹 Remove empty lines

- 🔁 Remove duplicates

- 🎯 Remove invalid IPs

- 📝 Strip unwanted text

- 🔌 Remove ports

- 📊 Sort IPv4 addresses

- 📁 Generate categorized result files

- ⚡ Process massive datasets efficiently

- 🌍 Run on Windows, Linux and macOS

---

## 🏗 Architecture

### Core Components

| Component | Responsibility |
|------------|---------------|
| 🚀 Program | Application workflow |
| 📂 DirHandler | Directory creation and file validation |
| 📊 UIHandler | Console UI and progress reporting |
| 🧠 LineRepository | In-memory data management |
| 💾 ResultHandler | Result file generation |
| 🔧 Utility | Parsing, regex extraction and helper methods |

### Processing Model

```text
Input Files
      ↓
LineRepository
      ↓
Statistics Engine
      ↓
Selected Operation
      ↓
ResultHandler
      ↓
Output File
```


---
## 📁 Supported Input Formats
IP File Normalizer is designed to work exclusively with plain text files.

| File Type | Supported |
|------------|------------|
| TXT | ✅ |
| CSV | ❌ |
| JSON | ❌ |
| XML | ❌ |
| XLSX | ❌ |
| XLS | ❌ |
| DOCX | ❌ |

---

### Supported Inputs Examples

```text
servers.txt
ips.txt
proxy-list.txt
targets.txt
```

### Unsupported Inputs Examples

```text
servers.csv
ips.json
targets.xlsx
data.xml
```
---

## ✨ Features

| Feature | Description |
|----------|------------|
| 🧹 NullFix | Remove empty lines |
| 🔁 DeDup | Remove duplicate lines and duplicate IPv4 addresses |
| 🎯 InFix | Remove invalid IPv4 addresses |
| 📝 LetOut | Remove unwanted text before and after IP addresses |
| 🔌 DePort | Remove ports from IP:PORT entries |
| 📈 SortUp | Sort IPv4 addresses ascending |
| 📉 SortDown | Sort IPv4 addresses descending |
| 🔥 Del4 | Remove all IPv4 addresses |
| 🔗 Merger | Merge multiple TXT files into one file |
| 🚀 ElonMod | Full automatic cleanup pipeline |
| 📊 Statistics Engine | Analyze dataset before processing |
| 📂 Smart Output Routing | Automatically store results in categorized folders |
| ⚡ Multi-File Processing | Process multiple TXT files simultaneously |
| 🧠 Duplicate Detection Engine | Detect duplicate lines and duplicate IPs |
| 📋 Port Analysis | Detect and report used ports |
| 💾 Large Dataset Support | Optimized for very large TXT files |

## 🧠 Processing Workflow

```text
TXT Files
    ↓
📂 Validation
    ↓
📥 Loading
    ↓
📊 Statistics Collection
    ↓
🛠 User Operation Selection
    ↓
⚙️ Processing
    ↓
💾 Result Generation
```

Before processing begins, every input file is validated:

✅ File exists
✅ TXT extension
✅ Non-empty file

Invalid files are reported individually before execution continues.

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

## ⚠️ Operation Priority Rules

Some operations have higher priority than others.

The application automatically resolves conflicting selections using the following rules:

### 🥇 Priority #1 — ElonMod

If all ElonMod components are selected:

```text
NullFix
DeDup
InFix
LetOut
DePort
SortUp
```

then the application switches to:

```text
🚀 ElonMod
```

and ignores all other selected options.

Example:

```text
✅ NullFix
✅ DeDup
✅ InFix
✅ LetOut
✅ DePort
✅ SortUp
✅ SortDown
✅ Del4
```

Result:

```text
🚀 ElonMod
```

All additional options are ignored.

---

### 🥈 Priority #2 — Merger

If `🔗 Merger` is selected, it overrides every other option except ElonMod.

Example:

```text
✅ Merger
✅ NullFix
✅ DeDup
✅ SortUp
```

Result:

```text
🔗 Merger
```

The application only merges input files.

---

### 🥉 Priority #3 — Sort Conflict Resolution

If both sorting modes are selected:

```text
📈 SortUp
📉 SortDown
```

then:

```text
📈 SortUp
```

takes priority automatically.

Example:

```text
✅ NullFix
✅ DeDup
✅ SortUp
✅ SortDown
```

Result:

```text
NullFix
DeDup
📈 SortUp
```

---

### Effective Priority Order

```text
🚀 ElonMod
      ↓
🔗 Merger
      ↓
📈 SortUp (when SortUp & SortDown are both selected)
      ↓
All Other Operations
```

These rules ensure deterministic behavior and prevent conflicting processing pipelines.

---

## 💻 User Interface
IP File Normalizer is a **CLI (Command-Line Interface)** application.

```text
Terminal / Console Based Application
```
### Supported Environments

- Windows Terminal
- Command Prompt (CMD)
- PowerShell
- Linux Terminal
- macOS Terminal

> **Note**
>
> This application does not provide a graphical user interface (GUI).
>
> All operations are performed through the command line / console.

---

## 📈 Statistics Engine
The application automatically collects approximate statistics before any modification is performed.

### Collected Metrics

- ☠️ Total Lines
- 🤣 Empty Lines
- 💩 Duplicate Lines
- 👎 Lines With Text
- 👙 Lines With Ports
- 📜 Used Ports
- 🥵 Total IPs
- 🍌 Duplicate IPs
- 👏 Invalid IPs
- 💎 Valid IPs

This allows users to understand dataset quality before selecting any operation.

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

At startup, the application automatically creates the following structure:

```text
Results
│
├── 1. ElonMod
├── 2. Merger
├── 3. MultiMod
├── NullFix
├── DeDup
├── InFix
├── LetOut
├── DePort
├── SortUp
├── SortDown
└── Del4
```

Result files are automatically routed to the appropriate folder based on the selected operation.

Example:

```text
IP File Normalizer_06-15-2025_12-45-11_ElonMod.txt
```

---

## ⚡ Performance

Built specifically for large TXT datasets.

### Technologies Used

- ConcurrentDictionary
- Parallel.ForEachAsync
- Async/Await
- MemoryMappedFile
- StreamReader
- StreamWriter
- Task-Based Processing

### Performance Optimizations

#### 📥 Fast Line Counting

Large files are scanned using MemoryMappedFile, allowing line counting without loading the entire file into memory.

#### ⚡ Parallel Duplicate Detection

Duplicate lines and duplicate IPs are detected using parallel processing across available CPU cores.

#### 💾 Buffered Result Writing

Result files are written asynchronously using buffered streams for maximum throughput.

#### 🧠 Lightweight Memory Usage

Only extracted metadata is stored for each line:

- Original Content
- IPv4 Address
- Port
- Additional Text
- IPv4 Byte Representation

### Optimized For

✅ Hundreds of thousands of lines

✅ Millions of IPv4 addresses

✅ Multi-file processing

✅ Large TXT datasets

✅ Cross-platform execution

✅ Self-contained deployments

---

## 🚀 Quick Start

### Clone

```bash
git clone https://github.com/Abel404Dev/IP-File-Normalizer.git
```

### Enter Directory

```bash
cd IP-File-Normalizer
```

### Build From Source

```bash
dotnet build
```

### Run From Source

```bash
dotnet run
```

### Run Release Build

#### Windows

```powershell
.\IP.File.Normalizer_windows_x64.exe
```

#### Linux

```bash
chmod +x IP.File.Normalizer_linux_x64
./IP.File.Normalizer_linux_x64
```

#### MacOS

```bash
chmod +x IP.File.Normalizer_macos_x64
./IP.File.Normalizer_macos_x64
```

---

## 📋 Usage

### Single TXT File

```text
C:\IPs\file1.txt
```

### Multiple TXT Files

```text
'C:\IPs\file1.txt',"C:\IPs\file2.txt" C:\IPs\file3.txt
```

### Available Operations

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

Proxy => 1.2.3.4
```

IP File Normalizer automatically extracts:

- IPv4 addresses
- Ports
- Leading text
- Trailing text

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
- 📦 Bulk TXT File Normalization
- ⚙️ Data Preparation Pipelines

---

## 🛠 Requirements

### For End Users

**None.**
Official releases are distributed as **Self-Contained Executables**.

You do **NOT** need to install:
- .NET SDK
- .NET Runtime
- Additional Dependencies

### Supported Platforms

| Platform | Supported |
|-----------|------------|
| 🪟 Windows | ✅ |
| 🐧 Linux | ✅ |
| 🍎 macOS | ✅ |

### For Developers

To build the project from source:
- .NET 10 SDK

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

⭐ If this project helped you, consider giving it a Star.

Made with ❤️ and ☕ for the networking & security community.

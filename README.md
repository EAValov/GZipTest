# GZipTest

Multithreading GZip archiver utility 

## Installation

### Requirements
[.NET Core 2.1](https://dotnet.microsoft.com/download/dotnet-core/2.1)

### Clone this repo

```bash
git clone "https://github.com/EAValov/GZipTest.git"
cd GZipTest
```

### Build

```bat
dotnet build -c Release
```

### Run test

```bat
dotnet test
```

## Usage

```bat
dotnet run -c Release -- [Mode] [Original file Path] [Processed file path]
```
### Example 
```bat
cd GZipTest
dotnet run -c Release -- "Compress" "D:\test.txt" "D:\test.txt.gz"
dotnet run -c Release -- "Decompress" "D:\test.txt.gz" "D:\test2.txt"
```


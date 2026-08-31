MacraggesHonourBlazor

A Blazor WebAssembly browser extension base template for .NET 10, built on top of the excellent Blazor.BrowserExtension package by mingyaulee.

Overview

This project serves as a foundation for building browser extensions using Blazor WebAssembly. It demonstrates a fully client-side .NET 10 Blazor application running inside a browser extension — no server required.

Use this as a starting point for your own Blazor-powered browser extension projects.

Built With
.NET 10
Blazor WebAssembly
Blazor.BrowserExtension by mingyaulee
Getting Started
Prerequisites
.NET 10 SDK (10.0.400 or later)
Visual Studio 2022 or later
Build
bash
dotnet publish -c Release

Load the bin/Release/net10.0/publish/browserextension/ folder as an unpacked extension in your browser.

Known Issues
WasmFingerprintAssets must be set to false in the .csproj due to a current incompatibility between .NET 10 fingerprinted asset filenames and Blazor.BrowserExtension 5.0.0.
Acknowledgements

A huge thank you to mingyaulee for creating and maintaining the Blazor.BrowserExtension package, which makes all of this possible. The broader Blazor and .NET open source community continues to push the boundaries of what's possible on the web.

For the Emperor.

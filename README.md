# Pickles 🥒

**A Revit-focused Dynamo package packed with 300+ utility nodes.**

**Current version:** `26.9.20XX`

Pickles is a **Dynamo package for Autodesk Revit**, providing a broad collection of practical utility nodes for everyday Revit automation and computational design workflows.

It is built **exclusively in C# using Dynamo's ZeroTouch architecture**, with the complete source code available in this repository.

## ✨ Features

### 🥒 Data Pickling — Pickles' signature feature

The node that gives the package its name. **Pickling** lets you take almost anything flowing through a graph — Elements, strings, booleans, numbers — and encode it into a compact, self-describing string that can be handed off, stored, and later **unpickled** back into a real object.

What makes it useful in practice:

* **Elements survive the round trip.** An Element is pickled as its `ElementId`, and `Unpickle` resolves it back through a document you supply (or the active document by default) — so it works just as well against a linked model as it does against the host file.
* **Three places to keep pickles**, depending on how long the data needs to live:
  * **On the graph itself** (`SavePicklesToGraph` / `LoadPicklesFromGraph`) — values are written into the Dynamo workspace and saved with it, so they're still there the next time the graph is opened, without any external file.
  * **To a local file** (`SavePicklesToFile` / `LoadPicklesFromFile`) — a simple `.tsv` you can share between graphs or archive.
  * **Keyed, so you can be selective** — `GetGraphKeys` lists what's stored, and `RemoveFromGraphByKeys` cleans up individual entries.
* **No external dependency.** No database, no config file, no naming convention to maintain by hand — just nodes.

It's a small idea (serialize → key → store → retrieve) applied consistently everywhere, and it's the piece of Pickles that doesn't really exist anywhere else in the Dynamo ecosystem.

### 🔗 Flexible document & link resolution

Most Revit-focused Dynamo nodes only ever look at the active document, which makes working with linked models awkward. Pickles nodes that touch a document — collectors, Rooms/Spaces, Sheets, Levels, coordinates, warnings, audits, exports, and more — share a single, flexible `docOrLinkInstance` input. Pass in:

* **Nothing** — the node falls back to the current document automatically.
* **A Document** — either a native Revit document or a Dynamo-wrapped one.
* **A `RevitLinkInstance`** — and the node resolves straight through to that link's underlying document.

One input, three accepted shapes, resolved internally by a shared document helper — so the same node works identically whether you're querying the host model or a link, with no separate "linked" versions of nodes to remember.

### ☀️ LocationHelper — a standalone sun-position engine

Beyond simple coordinate storage, `LocationHelper` is a small solar-geometry calculator built from scratch: given a latitude, longitude and timezone (or a Revit `SiteLocation`), it computes true solar declination, the equation of time, hour angle, and solar altitude/azimuth to return a real sun direction vector for any date and time — including optional daylight-saving rules.

* **Works with or without Revit.** `GetInternalSunDirection` returns the sun vector in a plain true-north-relative coordinate system, no document required.
* **Or aligned to your project.** `GetRevitSunDirection` takes the same calculation and rotates it into the document's actual **Project North**, using the model's True North angle — so the vector points the right way on your specific model, not just geographically.
* Useful for shading studies, solar-access checks, or any geometry that needs to react to sun position, without relying on Revit's own sun path UI.

### 🧩 300+ Utility Nodes

Pickles provides over **300 general-purpose Revit and Dynamo utility nodes**, organized into a handful of focused categories:

| Category | Covers |
| --- | --- |
| **Revit elements & documents** | Families, FamilyInstances/Types/Symbols/Parameters, Family Documents, Adaptive Components, Groups, Rooms, Spaces, Areas, Levels, Floors, Ceilings, Design Options, Worksets, Scope Boxes, Regions, RevitLinkInstances/Types |
| **Views, sheets & documentation** | Views, View Templates, Viewports, Sheets, Sheet Sets, View Schedules & Schedule Fields, Shared Parameters, Spec Types |
| **Collection & document utilities** | Category/Class collectors (host or link-aware), Coordinates, Warnings, Audit, Export, generic Element helpers |
| **Data & pickling** | Pickling/Unpickling, Colour, DateTime, Location/solar math, generic List and Math helpers, String utilities |
| **Application & system** | Clipboard, File and Directory I/O, URLs, Dynamo session info, OS/system helpers |
| **Geometry** | Points, Vectors, PolyCurves, Surfaces, Bounding Boxes, and general geometry modifiers |
| **Scripting & flow control** | Flow-control helpers, Timers, and UI prompts for scripted graphs |

The goal is simple: provide useful, predictable nodes that make working with Revit and Dynamo easier without unnecessary complexity.

### 🏗️ Revit Tools

Pickles includes a dedicated collection of Revit-focused functionality, providing access to a wide variety of Revit API operations through Dynamo — from element collection and family/type management to sheets, schedules, worksets, design options, and linked-model queries (see the coverage table above).

### 🎨 UI Library

Pickles includes a custom UI library supporting more advanced Dynamo workflows, including:

* **A custom dropdown framework**, with 20+ Revit-aware selectors (Documents, Families, Sheets, Views, View Templates, Scope Boxes, Title Blocks, Print Settings and more) built on a shared factory, plus general-purpose dropdowns like file filters and NATO letters.
* **A Dynamo View Extension** that backs the graph-level pickling storage and keeps node display names in sync.
* **Custom WPF interfaces** for message and input prompts used across the package.
* Additional UI components and utilities.

## 📦 Installation

Pickles is available through the **Dynamo Package Manager**.

It currently supports:

| Revit Version | Supported |
| ------------- | :-------: |
| Revit 2025    |     ✅     |
| Revit 2026    |     ✅     |
| Revit 2027    |     ✅     |

Search for **Pickles** in the Dynamo Package Manager and install the package corresponding to your Revit version.

> **Note:** Pickles packages are version-specific to Revit. Make sure you install the package version intended for your Revit version.

## 💻 Development

Pickles is written entirely in **C# / ZeroTouch**.

The full source code is available in this repository for anyone interested in exploring, contributing to, or building upon the project.

The project is intended to remain focused on **production-oriented Revit and Dynamo tooling**, with an emphasis on readable, maintainable and domain-appropriate code.

## 📄 License

Pickles is released under the **MIT License**.

You are free to use, modify and distribute the software in accordance with the terms of the license.

## 🔗 Links

**Website / Tutorials:**
https://www.youtube.com/aussiebimguru

**Source Code:**
https://github.com/aussieBIMguru/Pickles

---

Made by **Gavin Nicholls (ex Crump) / Aussie BIM Guru** 🥒
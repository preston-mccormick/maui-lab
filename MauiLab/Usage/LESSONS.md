# Usage lessons

## One native SQLite for the usage log and Entity Framework

Entity Framework Core 11 takes `SQLitePCLRaw` 3.0.5 through `SQLitePCLRaw.bundle_e_sqlite3` and the `SQLite` 3.53.4 package. `bundle_green` stops at 2.1.11, so it cannot be the shared pin. `sqlite-net-pcl` 1.11 speaks raw 3 and also pulls `SourceGear.sqlite3`, which ships the same `e_sqlite3` native library. That second copy is excluded. The usage file stays `usage.db` and the contact book stays `contacts.db`.

On iOS, `sqlite-net` links the system `libsqlite3` and its static constructor selects that provider. `Batteries_V2.Init()` selects the bundled `e_sqlite3.a` instead. A connection opened by one copy and stepped by the other dies in `sqlite3PcacheTruncate` (the page-cache pointer is a function inside the other library). iOS therefore does not link `e_sqlite3.a` and sets the system provider. Apple's library does not export `sqlite3_enable_load_extension`, so `Platforms/iOS/sqlite_load_extension_stub.c` defines it and returns an error. Every other target still calls `Batteries_V2.Init()`.

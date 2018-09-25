# Version History

## Pending

Add changes here when they're committed to the `master` branch. Move them to "Released" once the version number
is updated in preparation for publishing an updated NuGet package.

Prefix the description of the change with `[major]`, `[minor]` or `[patch]` in accordance with [SemVer](http://semver.org).

## Released

### 1.0.0 RC3

* **Breaking** Rename `ITrace` to `ITraceSpan`; rename all related types.
* Fix bug causing top-level URL to be overwritten by MVC child actions.
* Limit size of tag names to 128 bytes and tag values to 4096 bytes. (Longer values will be truncated.)
* Fix exception when ASP.NET child actions were traced.

### 1.0.0 RC2

* Add `NullTraceProvider`.
* Support `X-B3-Flags: 1`.

### 0.1.0

* Initial release.

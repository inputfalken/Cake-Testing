# Cake Testing
## Build Status
[![Build status](https://ci.appveyor.com/api/projects/status/v398iuf0qfjsmh8o?svg=true)](https://ci.appveyor.com/project/inputfalken/cake-testing)
## Examples
### With Arguments

```PowerShell
  .\cake.ps1 -ScriptArgs @("-Dist=debugDist", "-Configuration=Debug")
```
### Default Arguments
* Dist: dist
* Configuration: Release

```PowerShell
  .\cake.ps1
```

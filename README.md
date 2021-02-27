# idRehashCSharp
idRehash by infogram and proteh, rewritten in C#, designed to work in both Windows and Linux.


NOTE: Requires [linoodle](https://github.com/PowerBall253/linoodle "linoodle") to be in the same directory as the executable for running on Linux.

## Usage

To generate a hash offset map, run:
```
[mono] idRehash.exe --getoffsets
```

Afterwards, you can run:
```
[mono] idRehash.exe
```
to replace the resources hashes in meta.resources.

# File Formats
<a name="header"></a>
- #### Graphics
  - [.gli](#gli)
  - [.glj](#glj)
  - [.pol](#pol)
  - [.mcf](#mcf)
  - [.smk](#smk)
- #### Data
  - [.dat](#dat)
  - [.csv](#csv)
- #### Sound
  - [.midi](#midi)
  - [.ogg](#ogg)
  - [.raw](#raw)
## Graphics

### gli <a name="gli"></a>
  
#### Main Header
```
5 Bytes ("string") - name of the GFX file
5 Bytes of unknown value
4 Bytes (int) - Archive file size
20 Bytes of unknown value
4 Bytes (int) - GFX files in this collection (-1)
29 Bytes of unknown value
4 Bytes (int) - size of data
```

#### File Info Header
After the main header there is information about each file.

```
4 Bytes (int) - Type ID of the file
1 Byte (bool) - Is it a GFX File or something else (text etc.)
8 Bytes ("string") - Name as \n terminated string
4 Bytes (int) - The position offset of the actual file in the .gli file
```

#### Image Format
Each individual file has a 76 Byte long header.
```
4 Bytes (int) - Unknown... probably the size of the header (now that I think about it....)
4 Bytes (int) - File size in bytes
4 Bytes (int) - Pixel width
4 Bytes (int) - Pixel height
```
After those 76 Bytes: Array of 16 Bytes the size of ^File size^ divided by 2 - The color array in the format of RGB565, 5 Bit Red, 6 Bit Green, 5 Bit Blue

[Back to top](#header)

### glj <a name="glj"></a>

Unknown

[Back to top](#header)
### pol <a name="pol"></a>

Unknown file structure - Contains a subset of all the airport character animations

[Back to top](#header)
### mcf <a name="mcf"></a>

Unknown file structure - Contains font data

[Back to top](#header)
### smk <a name="smk"></a>
Contains animation data.

[See](https://en.wikipedia.org/wiki/Smacker_video)  the wiki for file information

[Back to top](#header)

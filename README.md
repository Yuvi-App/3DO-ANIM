# 3DO-ANIM

Simple program that takes a input ANIM file, and a folder of CELs, to create a new ANIM file.

Created this for a translation project, after much trouble with another very old 3DO tool.

**COMMANDS**
-origanim    | .ANIM FILE    | Set to the orig .ANIM file you wish to create

-inputceldir | DIR PATH      | Set to folder that contains the CELS for the new .ANIM file. These can be generated by Trapexit's 3IT tool. Ensure they are numbered at the end of filename. EX: TEST00.CEL TEST01.CEL

-includeplut | TRUE or FALSE | If TRUE it will write each CEL PLUT into the ANIM. This helps with palette issues.

-overwrite   | TRUE or FALSE | If TRUE we will overwrite the Orig ANIM File, else it gets built into \built\

**Example Commands**
.\3DO-ANIM.exe -origanim C:\GAME\ANIM\TEST.ANIM -inputceldir C:\FOLDEROFCELS -includeplut true -overwrite false
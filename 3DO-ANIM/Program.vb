Imports System
Imports System.IO
Imports System.Security.Cryptography

Module Program
    Dim IncludePLUT As Boolean = Nothing
    Dim CELInsertedintoANIM = 0

    Sub Main(args As String())
        Dim clArgs() As String = Environment.GetCommandLineArgs()
        Dim BuiltFolder = "built\"
        Dim OrigAnimFile As String = String.Empty
        Dim LocationofCELS As String = String.Empty
        Dim SavetoOrigAnim As Boolean = Nothing

        If clArgs.Count() = 9 Then
            For i As Integer = 1 To 7 Step 2
                If clArgs(i) = "-origanim" Then
                    OrigAnimFile = clArgs(i + 1)

                ElseIf clArgs(i) = "-inputceldir" Then
                    LocationofCELS = clArgs(i + 1)

                ElseIf clArgs(i) = "-includeplut" Then
                    If clArgs(i + 1).ToUpper = "FALSE" Then
                        IncludePLUT = False
                    ElseIf clArgs(i + 1).ToUpper = "TRUE" Then
                        IncludePLUT = True
                    Else
                        Console.WriteLine("IncludePLUT missing parameter... ABORTING")
                        Exit Sub
                    End If

                ElseIf clArgs(i) = "-overwrite" Then
                    If clArgs(i + 1).ToUpper = "FALSE" Then
                        SavetoOrigAnim = False
                    ElseIf clArgs(i + 1).ToUpper = "TRUE" Then
                        SavetoOrigAnim = True
                    Else
                        Console.WriteLine("Overwrite missing parameter... ABORTING")
                        Exit Sub
                    End If
                End If
            Next
        ElseIf clArgs.Count() = 2 Then
            If clArgs(1) = "-help" Then
                Console.WriteLine("-origanim | .ANIM FILE | Set to the orig .ANIM file you wish to create")
                Console.WriteLine("")
                Console.WriteLine("-inputceldir | DIR PATH | Set to folder that contains the CELS for the new .ANIM file. These can be generated by Trapexit's 3IT tool. Ensure they are numbered at the end of filename. EX: TEST00.CEL TEST01.CEL")
                Console.WriteLine("")
                Console.WriteLine("-includeplut | TRUE or FALSE | If TRUE it will write each CEL PLUT into the ANIM. This helps with palette issues.")
                Console.WriteLine("")
                Console.WriteLine("-overwrite | TRUE or FALSE | If TRUE we will overwrite the Orig ANIM File, else it gets built into \built\ ")
                Exit Sub
            End If
        Else
            Console.WriteLine("Missing Arguments, -help to see")
            Exit Sub
        End If

        'DEBUG STUFF
        'Console.WriteLine(OrigAnimFile)
        'Console.WriteLine(LocationofCELS)
        'Console.WriteLine(IncludePLUT)
        'Console.WriteLine(SavetoOrigAnim)

        'lets ensure the arguments are valid
        If File.Exists(OrigAnimFile) = False Then
            Console.WriteLine("Orginal ANIM file does not exist... ABORTING")
            Exit Sub
        End If
        Dim ANiMCheckByte As Int32 = 1296649793
        Using br As BinaryReader = New BinaryReader(File.Open(OrigAnimFile, FileMode.Open))
            If br.ReadInt32 <> ANiMCheckByte = True Then
                Console.WriteLine("WARNING: This file may not be a 3DO .ANIM file...")
            End If
        End Using

        'Setup DIR if not saving to orig dir.
        If SavetoOrigAnim = False Then
            If Directory.Exists(BuiltFolder) = False Then
                Directory.CreateDirectory(BuiltFolder)
            End If
        End If

        'Get ANIM Header
        Dim AnimHeaderBytes = GetAnimHeader(OrigAnimFile)

        'Check if ANIM file exist then delete if so
        Dim ANIMWriteFile = ""
        If SavetoOrigAnim = True Then
            If File.Exists(OrigAnimFile) = True Then
                File.Delete(OrigAnimFile)
            End If
            ANIMWriteFile = OrigAnimFile
        Else
            Dim NewAnimBuiltFile = BuiltFolder & Path.GetFileName(OrigAnimFile)
            If File.Exists(NewAnimBuiltFile) = True Then
                File.Delete(NewAnimBuiltFile)
            End If
            ANIMWriteFile = NewAnimBuiltFile
        End If


        'Write ANIM Header to new file
        WriteAnimeHeader(ANIMWriteFile, AnimHeaderBytes)

        'Lets Get them CELS into the ANIM
        For Each F In Directory.GetFiles(LocationofCELS, "*.CEL")
            Dim CELFilenameWOEXT = Path.GetFileNameWithoutExtension(F).ToUpper
            If CELFilenameWOEXT.EndsWith("00") Or CELFilenameWOEXT.EndsWith("0") And CELFilenameWOEXT.EndsWith("10") = False Then
                WriteFirstCELtoANIM(ANIMWriteFile, F)
            Else
                WriteROCELtoANIM(ANIMWriteFile, F)
            End If
        Next

        Console.WriteLine("Created ANIM (" & ANIMWriteFile & ") with " & CELInsertedintoANIM & " CELS inserted.")
    End Sub


    ' FUNCTIONS

    Public Function GetAnimHeader(ByVal OrigAnimFile As String)
        Dim ANIMHeaderBytes
        Using br As BinaryReader = New BinaryReader(File.Open(OrigAnimFile, FileMode.Open))
            ANIMHeaderBytes = br.ReadBytes(&H30)
        End Using
        Return ANIMHeaderBytes
    End Function

    Public Function WriteAnimeHeader(ByVal NewAnimFile As String, ByVal Bytes2Write As Byte())
        Using bw As BinaryWriter = New BinaryWriter(File.Open(NewAnimFile, FileMode.Create))
            bw.Write(Bytes2Write)
        End Using
    End Function

    Public Function WriteFirstCELtoANIM(ByVal NewAnimFile As String, ByVal FirstCEL As String)
        Dim File0Bytes = File.ReadAllBytes(FirstCEL)
        Using bw As BinaryWriter = New BinaryWriter(File.Open(NewAnimFile, FileMode.Append))
            bw.Write(File0Bytes)
        End Using
        CELInsertedintoANIM += 1
    End Function

    Public Function WriteROCELtoANIM(ByVal NewAnimFile As String, ByVal CEL As String)
        Dim Bytes2Write As Byte()
        If IncludePLUT = True Then
            Using br As BinaryReader = New BinaryReader(File.Open(CEL, FileMode.Open))
                br.BaseStream.Seek(&H50, SeekOrigin.Begin)
                Dim CheckString = System.Text.Encoding.UTF8.GetString(br.ReadBytes(2))
                If CheckString = "PL" Then
                    br.BaseStream.Seek(&H50, SeekOrigin.Begin)
                    Bytes2Write = br.ReadBytes(br.BaseStream.Length - br.BaseStream.Position)
                Else
                    Console.WriteLine("CANNOT LOCATED PLUT FOR " & CEL & " ... ABORTING")
                    End
                End If
            End Using
        Else
            Using br As BinaryReader = New BinaryReader(File.Open(CEL, FileMode.Open))
                br.BaseStream.Seek(&H50, SeekOrigin.Begin)
                Dim CheckString = System.Text.Encoding.UTF8.GetString(br.ReadBytes(2))
                If CheckString = "PL" Then
                    br.BaseStream.Seek(&H9C, SeekOrigin.Begin)
                    CheckString = System.Text.Encoding.UTF8.GetString(br.ReadBytes(2))
                    If CheckString = "PD" Then
                        br.BaseStream.Seek(&H9C, SeekOrigin.Begin)
                        Bytes2Write = br.ReadBytes(br.BaseStream.Length - br.BaseStream.Position)
                    Else
                        Console.WriteLine("CANNOT LOCATED PDAT FOR " & CEL & " ... ABORTING")
                        End
                    End If

                Else CheckString = "PD"
                    br.BaseStream.Seek(&H50, SeekOrigin.Begin)
                    Bytes2Write = br.ReadBytes(br.BaseStream.Length - br.BaseStream.Position)
                End If
            End Using
        End If

        Using bw As BinaryWriter = New BinaryWriter(File.Open(NewAnimFile, FileMode.Append))
            bw.Write(Bytes2Write)
        End Using
        CELInsertedintoANIM += 1
    End Function
End Module

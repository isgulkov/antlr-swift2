java -jar antlr-4.7-complete.jar Swift.g4 -o src
"C:\Program Files\MSBuild\14.0\Bin\MSBuild.exe" src\SwiftTranslator.sln
md res /s
xcopy src\bin\Debug\* res /y

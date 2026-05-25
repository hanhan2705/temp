Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead("c:\Users\MyPC\deviceManagement_claude\DOCX\1. FinalProject (2).docx")
$entry = $zip.Entries | Where-Object { $_.FullName -eq "word/document.xml" }
$stream = $entry.Open()
$reader = New-Object System.IO.StreamReader($stream)
$xml = $reader.ReadToEnd()
$reader.Close()
$zip.Dispose()
$text = [System.Text.RegularExpressions.Regex]::Replace($xml, "<.*?>", " ")
$text = [System.Text.RegularExpressions.Regex]::Replace($text, "\s+", " ")
$text | Out-File -FilePath "c:\Users\MyPC\deviceManagement_claude\DOCX\extracted_text.txt" -Encoding utf8

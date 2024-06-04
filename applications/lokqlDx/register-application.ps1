# Replace with the actual path to your application

$appPath = "C:\tools\lokqldx\lokqldx.exe" 


$progId = "kustoloco.lokqldx"
$extension = ".lokql"


# Create a value for this key that points to the ProgId
New-Item -Path "HKCU:\Software\$extension" -Force | New-ItemProperty -Name "(Default)" -Value $progId -Force

# Create a new key for the ProgId
New-Item -Path "HKCR:\$progId" -Force | New-ItemProperty -Name "(Default)" -Value $appPath -Force

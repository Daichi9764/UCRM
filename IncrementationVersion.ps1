# Chemin vers le fichier app.xaml.cs
$filePath = ".\KNXBoostDesktop\KNXBoostDesktop\app.xaml.cs"

# Lire tout le contenu du fichier
$fileContent = Get-Content $filePath

# Trouver la ligne contenant appBuild et incrémenter sa valeur
$fileContent = $fileContent -replace '(int AppBuild = )(\d+)', { param($matches) $matches[1] + ([int]$matches[2] + 1) }

# Écrire le contenu modifié dans le fichier
$fileContent | Set-Content $filePath

Write-Output "appBuild version incremented in $filePath"

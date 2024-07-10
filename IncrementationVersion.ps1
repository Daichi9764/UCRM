# Chemin vers le fichier app.xaml.cs
$filePath = ".\KNXBoostDesktop\KNXBoostDesktop\app.xaml.cs"

# Lire tout le contenu du fichier
$fileContent = Get-Content $filePath

# Initialiser une liste pour stocker le contenu modifié
$newContent = @()

# Parcourir chaque ligne du fichier
foreach ($line in $fileContent) {
    if ($line -match '^(.*public static readonly int AppBuild = )(\d+)(;.*)$') {
        # Extraire la valeur actuelle d'AppBuild
        $currentValue = [int]$matches[2]
        # Incrémenter la valeur
        $newValue = $currentValue + 1
        # Remplacer la ligne par la nouvelle valeur tout en conservant l'indentation
        $line = "$($matches[1])$newValue$($matches[3])"
    }
    # Ajouter la ligne (modifiée ou non) à la nouvelle liste de contenu
    $newContent += $line
}

# Écrire le contenu modifié dans le fichier
$newContent | Set-Content $filePath

Write-Output "AppBuild version incremented in $filePath"

#!/bin/bash

# Chemin vers le fichier app.xaml.cs
filePath="./KNXBoostDesktop/KNXBoostDesktop/app.xaml.cs"

# Lire tout le contenu du fichier
fileContent=$(cat $filePath)

# Initialiser une nouvelle variable pour stocker le contenu modifié
newContent=""

# Parcourir chaque ligne du fichier
while IFS= read -r line
do
    if [[ $line =~ (public\ static\ readonly\ int\ AppBuild\ =\ )([0-9]+)(;) ]]; then
        currentValue=${BASH_REMATCH[2]}
        newValue=$((currentValue + 1))
        line="${BASH_REMATCH[1]}${newValue}${BASH_REMATCH[3]}"
    fi
    newContent+="$line"$'\n'
done <<< "$fileContent"

# Écrire le contenu modifié dans le fichier
echo "$newContent" > $filePath

echo "AppBuild version incremented in $filePath"

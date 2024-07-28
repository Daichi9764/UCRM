## ⚙️ 2.2. Menu Paramètres
### 2.2.1 Paramètres généraux de l’application

![image](https://github.com/user-attachments/assets/97e8f6af-8aee-486d-aebe-129d404fdb6d)

La section "**Général**" vous permet de configurer les paramètres principaux de l'application pour mieux répondre à vos besoins.

1. **Thème :** Vous pouvez choisir le thème visuel de l'application en utilisant l'option "**Thème**". Par défaut, l'application utilise un thème clair, mais vous pouvez sélectionner le thème sombre disponible en cliquant sur le menu déroulant et en sélectionnant le thème sombre.

2. **Langue de l’application :** L’option "**Langue de l'application**" vous permet de sélectionner la langue dans laquelle l'interface utilisateur de l'application est affichée. Par défaut, l'application est en français (FR - Français). Cependant, si vous préférez utiliser une autre langue, vous pouvez choisir parmi les options disponibles en cliquant sur le menu déroulant et en sélectionnant la langue souhaitée.

3. **Mise à l’échelle :** Enfin, vous pouvez ajuster la mise à l'échelle de l'interface via l'option "**Mise à l'échelle**". Cette fonctionnalité permet de modifier la taille de l'interface pour améliorer la lisibilité. Vous pouvez ajuster le pourcentage de mise à l'échelle en faisant glisser le curseur. La plage de réglage va de 50% à 300%, avec le pourcentage actuel affiché au-dessus du curseur.

Pour enregistrer les modifications apportées aux paramètres, cliquez sur « **Sauvegarder** » en bas de la fenêtre paramètres.


### 📝 2.2.2 Paramètres de la correction
#### 2.2.2.1	Traduction des adresses de groupe
#### 2.2.2.2	Suppression des adresses de groupe non utilisées

### 2.2.3	✅ Paramètres d’inclusion

![image](https://github.com/user-attachments/assets/5331f21c-3b1c-4039-ab77-5eec475bb286)

L’onglet « **inclusions** » permet à l’utilisateur d’entrer une liste de mots ou de portions de phrases qui seront automatiquement incluses dans l’adresse de groupe corrigée si le mot est trouvé dans l’adresse originale.

![image](https://github.com/user-attachments/assets/75f9af12-ef77-421d-906d-eb8407400bf5)

Supposons que nous souhaitions conserver l’information « *PlanDeTravail* » sur l’adresse de groupe sélectionnée sur l’exemple ci-dessus.

![image](https://github.com/user-attachments/assets/552c4dba-3cb8-4f4a-adf6-46e82660f8f2)

Rendez-vous dans le menu paramètres, dans l’onglet inclusions. Entrez le mot ou la phrase à conserver. Appuyez sur la touche « *Entrée* ».

![image](https://github.com/user-attachments/assets/b77a5342-dd1e-4b74-bf2c-19c472f2c2a2)

Le mot a été ajouté à la liste d’inclusion (qui n’est pas sensible à la casse) et est activé par défaut.

![image](https://github.com/user-attachments/assets/d31ec684-3a63-4d66-802c-11a97aa25199)

En sauvegardant les modifications apportées aux paramètres, un bouton apparaît sur la fenêtre principale afin de, si besoin, recharger le projet en prenant en compte les nouveaux paramètres. A noter cependant que si vous avez modifié manuellement une adresse de groupe dans KNX Boost Desktop, cette modification sera perdue en rechargeant le projet.

![image](https://github.com/user-attachments/assets/fe483a83-0c93-45d4-a6d4-e4b7d07df1b1)

Après avoir rechargé le projet, il est possible de constater que lorsque l’adresse initiale contenait le mot à inclure, ce mot se retrouve automatiquement dans l’adresse corrigée. De cette manière, la correction ne fait perdre aucune information importante de l’adresse originale.



![image](https://github.com/user-attachments/assets/edc95f68-ddac-4776-8dfd-60e1e2a10dc1)

**Utilisation du caractère ‘\*’ pour inclure des mots similaires :** Vous pouvez utiliser le caractère ‘\*’ pour inclure facilement toute une série de mots qui se ressemblent. Par exemple, si vous voulez inclure tous les mots qui commencent par ‘*test*’, il suffit d'écrire ‘*test\**’. Cela inclura automatiquement des mots comme ‘*test1*’, ‘*test2*’, ‘*testXYZ*’, etc.
Le caractère ‘\*’ peut également être placé à n’importe quel endroit d’un mot pour représenter une partie variable. Par exemple, si vous écrivez ‘*XX_\*_YY*’, cela inclura des mots comme ‘*XX_123_YY*’, ‘*XX_ABCDEF_YY*’, et ainsi de suite.
En utilisant le caractère ‘\*’, vous pouvez facilement spécifier des groupes de mots similaires sans avoir à les écrire tous individuellement.

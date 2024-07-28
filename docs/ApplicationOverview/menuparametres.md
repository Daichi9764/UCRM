## 2.2. ⚙️ Menu Paramètres

2.2. ⚙️ [Menu paramètres](ApplicationOverview/menuparametres.md)

      --> 2.2.1. 📤 [Paramètres généraux de l’application](#parametres-generaux)
      
      --> 2.2.2. 📝 [Paramètres de la correction](#correction)

      --> 2.2.2. ✅ [Paramètres d'inclusion](#informations)

      --> 2.2.3. 🖥 [Paramètres de l’application](#paramètres-de-lapplication)

      --> 2.2.4. 🪲 [Débogage](#débogage)

      --> 2.2.5. 💡 [Informations](#informations)

### 2.2.1 📤 Paramètres généraux de l’application

![image](https://github.com/user-attachments/assets/97e8f6af-8aee-486d-aebe-129d404fdb6d)

La section "**Général**" vous permet de configurer les paramètres principaux de l'application pour mieux répondre à vos besoins.

1. **Thème :** Vous pouvez choisir le thème visuel de l'application en utilisant l'option "**Thème**". Par défaut, l'application utilise un thème clair, mais vous pouvez sélectionner le thème sombre disponible en cliquant sur le menu déroulant et en sélectionnant le thème sombre.

2. **Langue de l’application :** L’option "**Langue de l'application**" vous permet de sélectionner la langue dans laquelle l'interface utilisateur de l'application est affichée. Par défaut, l'application est en français (FR - Français). Cependant, si vous préférez utiliser une autre langue, vous pouvez choisir parmi les options disponibles en cliquant sur le menu déroulant et en sélectionnant la langue souhaitée.

3. **Mise à l’échelle :** Enfin, vous pouvez ajuster la mise à l'échelle de l'interface via l'option "**Mise à l'échelle**". Cette fonctionnalité permet de modifier la taille de l'interface pour améliorer la lisibilité. Vous pouvez ajuster le pourcentage de mise à l'échelle en faisant glisser le curseur. La plage de réglage va de 50% à 300%, avec le pourcentage actuel affiché au-dessus du curseur.

Pour enregistrer les modifications apportées aux paramètres, cliquez sur « **Sauvegarder** » en bas de la fenêtre paramètres.<br>
<br>
<br>
<br>
<br>
### 2.2.2 📝 Paramètres de la correction
#### 2.2.2.1	Traduction des adresses de groupe
#### 2.2.2.2	Suppression des adresses de groupe non utilisées<br>
<br>
<br>
<br>
<br>
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

Après avoir rechargé le projet, il est possible de constater que lorsque l’adresse initiale contenait le mot à inclure, ce mot se retrouve automatiquement dans l’adresse corrigée. De cette manière, la correction ne fait perdre aucune information importante de l’adresse originale.<br>
<br>
<br>
<br>
![image](https://github.com/user-attachments/assets/edc95f68-ddac-4776-8dfd-60e1e2a10dc1)

**Utilisation du caractère ‘\*’ pour inclure des mots similaires :** Vous pouvez utiliser le caractère ‘\*’ pour inclure facilement toute une série de mots qui se ressemblent. Par exemple, si vous voulez inclure tous les mots qui commencent par ‘*test*’, il suffit d'écrire ‘*test\**’. Cela inclura automatiquement des mots comme ‘*test1*’, ‘*test2*’, ‘*testXYZ*’, etc.

Le caractère ‘\*’ peut également être placé à n’importe quel endroit d’un mot pour représenter une partie variable. Par exemple, si vous écrivez ‘*XX_\*_YY*’, cela inclura des mots comme ‘*XX_123_YY*’, ‘*XX_ABCDEF_YY*’, et ainsi de suite.

En utilisant le caractère ‘\*’, vous pouvez facilement spécifier des groupes de mots similaires sans avoir à les écrire tous individuellement.

![image](https://github.com/user-attachments/assets/a611d67e-5c0b-411f-ba6e-10ce1be961d6)

Lorsque vous ajoutez un mot contenant le caractère ‘\*’ et que celui-ci recouvre des mots déjà présents dans la liste, les mots couverts sont automatiquement désactivés. Sur l’exemple ci-dessus, ‘*test1*’ et ‘*test*’ peuvent être obtenus grâce à ‘*test\**’. Ils ont donc été désactivés automatiquement à l’ajout de ‘*test\**’.

![image](https://github.com/user-attachments/assets/eb367ccd-eeb4-497c-8284-fa7afdf254f7)

Si vous ne souhaitez désormais inclure uniquement ‘*test1*’, il vous suffit de le cocher. Cela désactivera cependant tout mot qui le recouvrait précédemment. Sur l’exemple ci-dessus, l’activation de ‘*test1*’ a automatiquement désactivé ‘*test\**’.<br>
<br>
<br>
<br>
![image](https://github.com/user-attachments/assets/9ef3eecb-33c7-48b0-aad2-61a21550a2f5)

**Exporter une liste d’inclusions :** Il est possible d’exporter une liste d’inclusions afin de l’utiliser sur un autre ordinateur équipé de KNX Boost Desktop. Pour cela, cliquez sur le bouton entouré sur l’image ci-dessus.

![image](https://github.com/user-attachments/assets/b3c6d413-d068-4c8b-b67f-4d8dafb96058)

Un menu s’ouvre afin de vous permettre d’enregistrer la liste d’inclusions à l’endroit où vous le souhaitez sur votre ordinateur.<br>
<br>
<br>
<br>
![image](https://github.com/user-attachments/assets/c29848b2-615d-4aa7-8223-7b78eca9e624)

Il est également possible d’importer une liste d’exclusions dans l’application. Pour cela, cliquez sur le bouton entouré sur l’image ci-dessus.

![image](https://github.com/user-attachments/assets/738800cf-7280-4bc3-b8cc-fe4484e428af)

Un menu s’ouvre afin de vous permettre de sélectionner la liste d’inclusions à importer depuis votre ordinateur. En cliquant sur « **ouvrir** », tous les mots que contenaient les fichiers ont été importés automatiquement dans l’application. Il suffira de cliquer sur « **sauvegarder** » pour enregistrer les nouvelles adresses dans l’application.<br>
<br>
<br>
<br>
<br>
### 2.2.4	🪲 Débogage

![image](https://github.com/user-attachments/assets/c625a8da-31c6-4b83-b4f0-bdac47ec5127)

L'onglet "**Débogage**" du menu des paramètres est conçu pour aider les utilisateurs à collecter des informations utiles pour le diagnostic et la résolution des problèmes rencontrés lors de l'utilisation de l'application. Cet onglet permet de configurer les données à inclure dans le fichier de débogage, facilitant ainsi le processus de support technique. Plusieurs options peuvent être cochées pour inclure plus ou moins d’informations issues du logiciel et de l’ordinateur.

1. **Inclure les informations sur le système d'exploitation :** Lorsque cette option est cochée, le fichier de débogage inclut des informations détaillées sur le système d'exploitation que vous utilisez. Cela peut inclure la version du système, les paramètres régionaux, et d'autres détails spécifiques au système d'exploitation. Ces informations sont essentielles pour identifier si un problème est lié à une particularité ou une configuration spécifique du système d'exploitation.
2. **Inclure les informations sur le matériel de l'ordinateur :** En activant cette option, vous permettez à l'application de collecter des données sur le matériel de votre ordinateur, telles que le processeur, la mémoire, la carte graphique, et d'autres composants matériels. Les informations matérielles aident à déterminer si les problèmes de performance ou de compatibilité sont liés au matériel de votre machine.
3. **Inclure les fichiers des projets importés depuis le lancement :** Cette option permet d'inclure dans le fichier de débogage les fichiers des projets que vous avez importés depuis le dernier lancement de l'application. En fournissant ces fichiers, vous aidez les développeurs à reproduire et analyser les problèmes spécifiques à vos projets, facilitant ainsi la résolution des bugs.
4. **Inclure la liste des adresses de groupe supprimées sur les projets :** Cette option, lorsqu'elle est cochée, ajoute au fichier de débogage une liste des adresses de groupe qui ont été supprimées de vos projets. Ces informations peuvent être cruciales pour comprendre les modifications apportées aux projets et pour diagnostiquer les problèmes liés aux modifications apportées par le logiciel.

Après avoir sélectionné les options appropriées, vous pouvez créer le fichier de débogage en cliquant sur le bouton situé en bas de l'onglet, intitulé "**Créer le fichier de débogage**". Ce fichier compilera toutes les informations sélectionnées et pourra être envoyé au support technique pour une analyse approfondie.<br>
<br>
<br>
<br>
<br>
### 2.2.5 💡 Informations

![image](https://github.com/user-attachments/assets/9ba77406-2630-483e-867f-fd3463ced050)

L'onglet "**Informations**" du menu des paramètres fournit des détails essentiels sur le logiciel, sa version, et les personnes impliquées dans son développement. En bas de l'onglet, une note souligne que le nom, les logos et toute image liée à KNX sont la propriété inaliénable de l'association KNX. Un lien vers le site web de l'association KNX est également fourni pour plus d'informations.

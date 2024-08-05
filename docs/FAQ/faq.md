# FAQ
## Sommaire

- [❓ **Certaines adresses de groupes n’ont pas été renommées par le logiciel, est-ce normal ?**](#q1)
- [📤 **Comment exporter le projet .knxproj avec les adresses de groupe modifiées ?**](#q2)
- [🛠️  **Le renommage des adresses de groupe présente des erreurs dans le métier, la fonctionnalité et/ou l’emplacement des objets des adresses de groupe. Que faire ?**](#q3)
- [📝 **Des éléments importants présents dans les adresses de groupes originales ne sont pas conservés par le logiciel. Comment les faire apparaître tout de même dans les adresses de groupe modifiées ?**](#q4)
- [🔧 **La modification des adresses de groupe a-t-elle un impact sur le fonctionnement de mon projet ?**](#q5)
<br><br>
---

### <a id="q1"></a> ❓ Certaines adresses de groupes n’ont pas été renommées par le logiciel, est-ce normal ? 
Il s'agit des adresses de groupes qui ne sont **liées à aucun objet du projet**. Ces adresses ne sont pas renommées par le logiciel en raison du manque d'informations les concernant. N'étant pas utiles au fonctionnement du projet dans l'état actuel,
un astérisque est ajouté au début de leur nom pour faciliter leur identification dans ETS en vue d'une suppression. Pour plus d'informations, veuillez vous référer à la section
[2.2.2.2	Suppression des adresses de groupe non utilisées](../ApplicationOverview/menuparametres.md#suppression-des-adresses-de-groupe-non-utilisees).

🔹🔹🔹

### <a id="q2"></a> 📤 Comment exporter le projet .knxproj avec les adresses de groupe modifiées ? 
Il n'est pour l'instant pas possible d'exporter le projet dans son intégralité pour une réinsertion dans ETS. 
Pour apporter des modifications au projet, il suffit d'**exporter les fichiers d'adresses de groupe modifiées** et de les réimporter dans le projet ETS correspondant, 
comme détaillé dans la section [3.5. 📤 Exporter les adresses de groupe modifiées](../UtilisationApplication/exporter-adresses-de-groupe-modifiees.md).

🔹🔹🔹


### <a id="q3"></a>🛠️  Le renommage des adresses de groupe présente des erreurs dans le métier, la fonctionnalité et/ou l’emplacement des objets des adresses de groupe. Que faire ? 
Le fonctionnement du logiciel s'appuie sur les informations de topologie et de bâtiment renseignées dans le projet sur ETS. Si ces informations ne sont pas correctement renseignées, les erreurs se répercuteront dans le renommage des adresses de groupe. **Veuillez donc structurer correctement votre projet avant l'utilisation du logiciel**.

🔹🔹🔹

### <a id="q4"></a>📝 Des éléments importants présents dans les adresses de groupes originales ne sont pas conservés par le logiciel. Comment les faire apparaître tout de même dans les adresses de groupe modifiées ?
* Si vous souhaitez conserver des éléments des adresses originales de manière **récurrente** dans le projet, vous pouvez le faire via l'utilisation des **« inclusions »** présentées la section [2.2.3 ✅ Paramètres d’inclusion](../ApplicationOverview/menuparametres.md#paramètres-dinclusion).

* Si vous souhaitez conserver des éléments plus **ponctuels**, vous pouvez utiliser directement la fonction de **renommage** en double-cliquant sur l'adresse renommée où un élément est manquant pour l'ajouter manuellement. Plus de détails sur la fonction de renommage dans la section  [3.4. 📝 Renommer manuellement des adresses de groupe](../UtilisationApplication/renommer-manuellement-des-adresses-de-groupe.md).

🔹🔹🔹

### <a id="q5"></a>🔧 La modification des adresses de groupe a-t-elle un impact sur le fonctionnement de mon projet ? 
L'utilisation du logiciel **n'impacte en rien le fonctionnement du projet**, car seuls les champs texte des adresses de groupe sont modifiés.

[← Retour](../README.md)

# FAQ
## Table of Contents

- [❓ **Some group addresses were not renamed by the software, is this normal?**](#q1)
- [📤 **How to export the .knxproj project with the modified group addresses?**](#q2)
- [🛠️ **The renaming of group addresses shows errors in the function, type, and/or location of the group address objects. What should I do?**](#q3)
- [📝 **Important elements present in the original group addresses are not retained by the software. How can they still appear in the modified group addresses?**](#q4)
- [🔧 **Does modifying group addresses impact the functionality of my project?**](#q5)
<br><br>
---

### <a id="q1"></a> ❓ Some group addresses were not renamed by the software, is this normal? 
These are group addresses that are **not linked to any project object**. These addresses are not renamed by the software due to a lack of information about them. Since they are not useful for the project's operation in its current state, an asterisk is added to the beginning of their name to facilitate their identification in ETS for deletion. For more information, please refer to the section
[2.2.2.2 Deleting unused group addresses](../ApplicationOverview/settingswindow.md#deletion-of-unused-group-addresses).

🔹🔹🔹

### <a id="q2"></a> 📤 How to export the .knxproj project with the modified group addresses? 
Currently, it is not possible to export the entire project for reimport into ETS. 
To make changes to the project, simply **export the modified group address files** and reimport them into the corresponding ETS project, 
as detailed in the section [3.5. 📤 Exporting modified group addresses](../UtilisationApplication/EN-export-modified-group-addresses.md

🔹🔹🔹

### <a id="q3"></a> 🛠️ The renaming of group addresses shows errors in the function, type, and/or location of the group address objects. What should I do? 
The software's operation relies on the topology and building information provided in the project on ETS. If this information is not correctly filled out, errors will be reflected in the renaming of group addresses. **Please ensure that your project is properly structured before using the software**.

🔹🔹🔹

### <a id="q4"></a> 📝 Important elements present in the original group addresses are not retained by the software. How can they still appear in the modified group addresses?
* If you want to retain elements from the original addresses on a **recurring basis** in the project, you can do so using the **"inclusions"** as described in the section [2.2.3 ✅ Inclusion Settings](../ApplicationOverview/settingswindow.md#inclusion-settings).

* If you want to retain more **occasional** elements, you can directly use the **renaming** function by double-clicking on the renamed address where an element is missing to add it manually. More details on the renaming function can be found in the section [3.4. 📝 Manually renaming group addresses](../UtilisationApplication/EN-manually-rename-group-addresse.md).

🔹🔹🔹

### <a id="q5"></a> 🔧 Does modifying group addresses impact the functionality of my project? 
The use of the software **does not impact the project's functionality**, as only the text fields of the group addresses are modified.


[← Go back](../README-EN.md)

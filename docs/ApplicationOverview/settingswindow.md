## 2.2. ⚙️ Settings Menu

--> 2.2.1. 📤 [General Application Settings](#general-application-settings)

--> 2.2.2. 📝 [Correction Settings](#correction-settings)

--> 2.2.3. ✅ [Inclusion Settings](#inclusion-settings)

--> 2.2.4. 🪲 [Debugging](#debugging)

--> 2.2.5. 💡 [Information](#information)<br>
<br>
<br>
<br>
<br>
### 2.2.1 📤 General Application Settings <a name="general-application-settings"></a>

![image](https://github.com/user-attachments/assets/97e8f6af-8aee-486d-aebe-129d404fdb6d)

The "**General**" section allows you to configure the main settings of the application to better suit your needs.

1. **Theme:** You can choose the visual theme of the application using the "**Theme**" option. By default, the application uses a light theme, but you can select the available dark theme by clicking on the drop-down menu and selecting the dark theme.

2. **Application Language:** The "**Application Language**" option allows you to select the language in which the application's user interface is displayed. By default, the application is in French (FR - Français). However, if you prefer to use another language, you can choose from the available options by clicking on the drop-down menu and selecting the desired language.

3. **Scaling:** Finally, you can adjust the interface scaling via the "**Scaling**" option. This feature allows you to change the size of the interface to improve readability. You can adjust the scaling percentage by sliding the slider. The adjustment range goes from 50% to 300%, with the current percentage displayed above the slider.

To save the changes made to the settings, click on “**Save**” at the bottom of the settings window.<br>
<br>
<br>
<br>
<br>
### 2.2.2 📝 Correction Settings <a name="correction-settings"></a>
![image](https://github.com/user-attachments/assets/15ce8fc1-5e56-4d8a-aa21-c9bd00dea14b)

The **Correction** menu allows you to configure correction settings according to your needs. It is divided into two sections: **Translation** and **Group address management**. The details are as follows:

#### 2.2.2.1 Translation of group addresses

By default, translation is disabled. It is used to translate the names of group addresses modified by KNX Boost Desktop. To enable translation, check the **Enable translation** option. When enabling translation for the first time, you will be prompted to enter a Deepl API key, which you can obtain for free on the Deepl website via a provided link. Once the API key is entered and the settings are saved for the first time, it will be retained.

The **Enable automatic language detection for translation** option is enabled by default. This option allows Deepl to automatically detect the language of the words to be translated. If automatic language detection is not 100% reliable, you can specify the source language for translation using the dropdown menu under **Translate source language**. 

The dropdown menu under **Translation destination language** allows you to select the language into which the group address names will be translated.

By clicking **Save**, the changes to the settings will be saved. If a project is already imported, a **Reload** button will appear. By clicking on it, the project will be reloaded with the updated settings.

![image](https://github.com/user-attachments/assets/3c5bb060-4549-49e8-a955-100225d715f5)

By clicking **Cancel**, the changes to the settings will be discarded and the last saved settings will be restored.

#### 2.2.2.2 Deletion of unused group addresses<a name="deletion-of-unused-group-addresses"></a><br>

By default, the **Delete unused addresses** checkbox is selected. This means that group addresses not linked to any participants will not be displayed in the main window, as they cannot be renamed by KNX Boost Desktop. With this option enabled, when you import the new group address file into ETS, the unlinked addresses will be prefixed with the * character for easier identification.

By clicking **Save**, the changes to the settings will be saved. If a project is already imported, a **Reload** button will appear. By clicking on it, the project will be reloaded with the updated settings.

![image](https://github.com/user-attachments/assets/3c5bb060-4549-49e8-a955-100225d715f5)

By clicking **Cancel**, the changes to the settings will be discarded and the last saved settings will be restored.

<br>
<br>
<br>

### 2.2.3 ✅ Inclusion Settings <a name="inclusion-settings"></a>

![image](https://github.com/user-attachments/assets/5331f21c-3b1c-4039-ab77-5eec475bb286)

The "**inclusions**" tab allows the user to enter a list of words or phrases that will be automatically included in the corrected group address if the word is found in the original address.

![image](https://github.com/user-attachments/assets/75f9af12-ef77-421d-906d-eb8407400bf5)

Suppose we want to keep the information “*PlanDeTravail*” in the group address selected in the example above.

![image](https://github.com/user-attachments/assets/552c4dba-3cb8-4f4a-adf6-46e82660f8f2)

Go to the settings menu, in the inclusions tab. Enter the word or phrase to keep. Press the "Enter" key.

![image](https://github.com/user-attachments/assets/b77a5342-dd1e-4b74-bf2c-19c472f2c2a2)

The word has been added to the inclusion list (which is not case-sensitive) and is enabled by default.

![image](https://github.com/user-attachments/assets/d31ec684-3a63-4d66-802c-11a97aa25199)

By saving the changes made to the settings, a button appears on the main window to reload the project if necessary, taking into account the new settings. Note, however, that if you manually modified a group address in KNX Boost Desktop, this modification will be lost when reloading the project.

![image](https://github.com/user-attachments/assets/fe483a83-0c93-45d4-a6d4-e4b7d07df1b1)

After reloading the project, you can see that when the initial address contained the word to be included, this word is automatically found in the corrected address. In this way, the correction does not lose any important information from the original address.<br>
<br>
<br>
<br>
![image](https://github.com/user-attachments/assets/edc95f68-ddac-4776-8dfd-60e1e2a10dc1)

**Using the ‘\*’ Character to Include Similar Words:** You can use the ‘\*’ character to easily include a whole series of similar words. For example, if you want to include all words that start with ‘*test*’, simply write ‘*test\**’. This will automatically include words like ‘*test1*’, ‘*test2*’, ‘*testXYZ*’, etc.

The ‘\*’ character can also be placed anywhere in a word to represent a variable part. For example, if you write ‘*XX\*YY*’, it will include words like ‘*XX123YY*’, ‘*XXABCDEFYY*’, and so on.

By using the ‘\*’ character, you can easily specify groups of similar words without having to write them all individually.

![image](https://github.com/user-attachments/assets/a611d67e-5c0b-411f-ba6e-10ce1be961d6)

When you add a word containing the ‘\*’ character and it covers words already present in the list, the covered words are automatically deactivated. In the example above, ‘*test1*’ and ‘*test*’ can be covered by ‘*test\**’. They were therefore automatically deactivated when ‘*test\**’ was added.

![image](https://github.com/user-attachments/assets/eb367ccd-eeb4-497c-8284-fa7afdf254f7)

If you now only want to include ‘*test1*’, simply check it. However, this will deactivate any word that previously covered it. In the example above, enabling ‘*test1*’ automatically deactivated ‘*test\**’.<br>
<br>
<br>
<br>
![image](https://github.com/user-attachments/assets/9ef3eecb-33c7-48b0-aad2-61a21550a2f5)

**Export an Inclusion List:** It is possible to export an inclusion list to use on another computer equipped with KNX Boost Desktop. To do this, click on the button circled in the image above.

![image](https://github.com/user-attachments/assets/b3c6d413-d068-4c8b-b67f-4d8dafb96058)

A menu opens to allow you to save the inclusion list wherever you want on your computer.<br>
<br>
<br>
<br>
![image](https://github.com/user-attachments/assets/c29848b2-615d-4aa7-8223-7b78eca9e624)

It is also possible to import an exclusion list into the application. To do this, click on the button circled in the image above.

![image](https://github.com/user-attachments/assets/738800cf-7280-4bc3-b8cc-fe4484e428af)

A menu opens to allow you to select the inclusion list to import from your computer. By clicking on “**open**”, all the words contained in the files have been automatically imported into the application. Simply click on “**save**” to save the new addresses in the application.<br>
<br>
<br>
<br>
<br>
### 2.2.4 🪲 Debugging <a name="debugging"></a>

![image](https://github.com/user-attachments/assets/c625a8da-31c6-4b83-b4f0-bdac47ec5127)

The "**Debugging**" tab of the settings menu is designed to help users collect useful information for diagnosing and troubleshooting issues encountered while using the application. This tab allows you to configure the data to be included in the debug file, thus facilitating the technical support process. Several options can be checked to include more or less information from the software and the computer.

1. **Include operating system information:** When this option is checked, the debug file includes detailed information about the operating system you are using. This can include the system version, locale settings, and other specific operating system details. This information is essential to identify if an issue is related to a particularity or specific configuration of the operating system.
2. **Include computer hardware information:** Enabling this option allows the application to collect data about your computer's hardware, such as the processor, memory, graphics card, and other hardware components. Hardware information helps determine if performance or compatibility issues are related to your machine's hardware.
3. **Include project files imported since launch:** This option includes in the debug file the project files you have imported since the last launch of the application. Providing these files helps developers reproduce and analyze issues specific to your projects, thus facilitating bug resolution.
4. **Include the list of deleted group addresses in projects:** This option, when checked, adds to the debug file a list of group addresses that have been deleted from your projects. This information can be crucial for understanding the modifications made to the projects and for diagnosing issues related to changes made by the software.

After selecting the appropriate options, you can create the debug file by clicking on the button at the bottom of the tab, titled "**Create debug file**". This file will compile all the selected information and can be sent to technical support for further analysis.<br>
<br>
<br>
<br>
<br>
### 2.2.5 💡 Information <a name="information"></a>

![image](https://github.com/user-attachments/assets/9ba77406-2630-483e-867f-fd3463ced050)

The "**Information**" tab of the settings menu provides essential details about the software, its version, and the people involved in its development. At the bottom of the tab, a note emphasizes that the name, logos, and any images related to KNX are the inalienable property of the KNX association. A link to the KNX association's website is also provided for more information.

[← Go back](../README-EN.md)

### Installation Guide: Move PlanScoreCard to system scripts directory, copy and edit launcher (optional)
* Open your system scripts directory, click "System Scripts", "Open Folder...", OK

![image](https://user-images.githubusercontent.com/78000769/171965379-e0658fb9-da77-4661-9ba0-52e568d18e9d.png)


* Cut/paste or drag/drop the directory you extracted from the .zip above into the PublishedScripts directory  

![image](https://user-images.githubusercontent.com/78000769/171965727-461e080a-306f-4819-a667-5ab46eb3fab3.png)


* Open the directory containing the PlanScoreCard software and right click PlanScoreCardLauncher.cs, select Copy

![image](https://user-images.githubusercontent.com/78000769/171966328-14f39ae5-c69e-4186-9c7f-7c71dd3b25f1.png)


* Go up a directory, back to the PublishedScripts directory and paste the PlanScoreCardLauncher.cs there

![image](https://user-images.githubusercontent.com/78000769/171969497-0b4fb95c-a5a6-49f5-991a-fd2a2b1b8a7e.png)


* Copy the name of the PlanScoreCard software directory

![image](https://user-images.githubusercontent.com/78000769/171970450-7352d92c-de7b-40fb-8f3d-be738791a4a2.png)


* Open the PlanScoreCardLauncher.cs file with Notepad, highlight "SubDirectory", right click and "Paste"

![image](https://user-images.githubusercontent.com/78000769/171969622-f9897a81-83dd-4929-9165-3ca247431670.png)


* Cut the "//" comment from that line of the file

![image](https://user-images.githubusercontent.com/78000769/171967588-518ea190-93e5-4e36-8c30-0975e831d625.png)


* Paste it onto the beginning the following line instead and save the file, close Notepad

![image](https://user-images.githubusercontent.com/78000769/171967903-c2ddb857-45e1-4f1a-8124-5eec884bfbca.png)


* Reopen the Scripts window in Eclipse, click System Scripts, select PlanScoreCardLauncher.cs and select "Run"

![image](https://user-images.githubusercontent.com/78000769/171968258-bb9e05eb-e306-4aee-bdea-08b91a7d01d2.png)

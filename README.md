# PlanScoreCard

##### Medical Affairs Applied Solutions ESAPI tool to create dosimetric ScoreCards and score plans.
##### Features:
* Quantitative piecewise linear scoring functions for each metric
  * optional: flag for point where "variation acceptable" sited on referenced protocol
  * optional: note section to site referenced protocol or justification for metric (points)
  * optional: qualitative colors and labels for metric points, ie: orange="Just OK"
* Compare multiple plans in multiple courses at the same time for a given patient
* Advanced scoring criteria supported
* Advanced structure matching logic with integrated customizable preloaded json structure dictionary
* In-metric Structure Boolean/expansions created dynamically for scoring 
  * optionally save back to Plan if script approval/writable (to auto-create optimization tuning structures)
* Normalize dose to max score
  * removes noise during plan comparison from suboptimal normalization impacting relative scores
  * re-normalized to max score plans saved into a new course
* Dose per fraction and number of fractions stored in scorecard
  * Linear scaling of all doses on scorecard possible if # fx and dose/fx not matching Eclipse
* Checkbox to annotate note explaining specific metric performance (for report printing)
* CSV or PDF output reports

### Main Screen
![MainScreen1 0 0](https://user-images.githubusercontent.com/78000769/169741084-0bb83fdc-69f4-4240-a193-e1e41db9c0df.png)

### ScoreCard Editor
![ScoreCardEditor1 0 0](https://user-images.githubusercontent.com/78000769/169741156-a27d6165-616a-4181-80a7-74ed7923e8eb.png)

### Structure Builder
![StructureBuilder1 0 0](https://user-images.githubusercontent.com/78000769/169741212-82f16a0d-1e11-4ee4-863d-ac2446486c5b.png)

### Installation Guide: Quick-Start
* Click "releases" on the right side of this window

![image](https://user-images.githubusercontent.com/78000769/171961163-8c195302-3da5-4262-b736-1a1f033c9db1.png)


* Find your Eclipse version, expand "Assests" and click the largest ".zip" file to download

![image](https://user-images.githubusercontent.com/78000769/171961712-54f94f81-e079-49cc-b0d5-821b2e56f7df.png)


* Locate the downloaded file and move it to a location that can be acessed by your Eclipse system

![image](https://user-images.githubusercontent.com/78000769/171962212-ca132966-3335-400a-9e4e-d448ecf729ab.png)


* Once the .zip file has been moved to where Eclipse can acccess, right click select "properties" and unblock (if blocked)

![ZipFileUnblockedPriorToExtract](https://user-images.githubusercontent.com/78000769/169857843-a342b6b8-5f6f-41d5-9b2e-32995778280d.png)


* Extract the .zip file to create a directory by right clicking the file and selecting "Extract All...", click next until finished

![image](https://user-images.githubusercontent.com/78000769/171962899-1f5adcdd-4e62-40d0-a6cf-342eff3ce981.png)


* In Ecllipse, click Tools from the menu bar and select "Scripts", click folder, then "Change Folder..."

![image](https://user-images.githubusercontent.com/78000769/171963870-6710d081-453a-4fcd-8c96-997ed816f0b2.png)


* Browse into the directory you created after extracting the file above and click "Open" 

![image](https://user-images.githubusercontent.com/78000769/171964365-6ade7378-4d06-4167-8c41-b4f90f013a77.png)


* Run the PlanScoreCardLauncher.cs

![image](https://user-images.githubusercontent.com/78000769/171964099-6a1c4aca-18c2-4140-b9b6-c1e1b28f58c6.png)


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


### Common Troubleshooting

##### error after accepting the EULA (legal agreement) because the software can't write to the config file over a network (UNC path)
This error:
![image](https://user-images.githubusercontent.com/78000769/171474668-a270a5c5-308e-4b37-86e6-18184eae8c48.png)
Can be worked around by maunaully editing the planscorecard.exe.config, set EULAAgree to "true" manually
![image](https://user-images.githubusercontent.com/78000769/171475445-79840679-78fb-4057-84b2-0a0174e918f4.png)


##### Unblock .zip prior to unzipping
![ZipFileUnblockedPriorToExtract](https://user-images.githubusercontent.com/78000769/169857843-a342b6b8-5f6f-41d5-9b2e-32995778280d.png)

##### Permissions on Directory with executable
![NTFSpermissionsDir](https://user-images.githubusercontent.com/78000769/169858109-43550451-13da-4de4-afff-893afecfd4cd.png)

## Current version: V2.5.6.X-06/01/2022
### Improvements:
N/A

### Bugfixes:
* Fixed unable to change metric type or edit metrics after clicking recalculate score (must restart PlanScoreCard and reload to edit)
* Fixed no longer real time plot updating in Metric Editor
* Fixed modified gradient index crashes if values left blank
* Fixed metrics, structures, and Rx from a previously opened scorecard are propagated to the editor
* Fixed change the Rx in the scorecard editor, back on scorecard page the Rx isn't updated
* Fixed ScoreCardBuilder Selecting metric value for variation does not update plot
* Fixed ScoreCardBuilder Possible to select multiple variation values in a single metric
* Fixed certain patients cause crash on launching PlanScoreCard (only seen in V17)
  
### Known issues:
* ScoreCardBuilder Intermittent color/label change also changes lowest color/label in qualitative label/edit color picker
* ScoreCardBuilder Copying metric without structure assigned crashes app
* StructureBuilder Reparse existing structurebuilder structures for editing (currently must delete and re-create)
 
### 2.X Feature roadmap:
* guided structure builder (aka: simple mode, to be in front of current advanced structure builder)
* reload current patient from Eclipse button
* command line interface to output plan scores to the CLI or automate generation of CSV/PDF
* Import/Export buttons on ScoreCard Builder to support opening and saving various file types
* optional DVH rendering below UI with horizontal(D@V)+vertical(V@D) scoring brackets per selected structure


## Changelog

## V2.5.0.6-05/19/2022
### Improvements:
![MainScreenChanges](https://user-images.githubusercontent.com/78000769/169741261-04f08e03-1d04-47dd-a3e4-93f7f0198f6a.png)
##### MainScreen UI improvements:

A. Not Validated for Clinical USE moved to title

B. The word Summary Removed

C. Summary moved to top of application.

D. Flags and Warnings turned into buttons and don't grow with the number of messages anymore.
![warning-errors](https://user-images.githubusercontent.com/78000769/169742259-6bd0ff3d-3586-4c9e-92d7-b4801224459e.png)

E. The word "Plan Scores:" is no longer on its own line.

F. Unit put on value.

G. EULA moved under Plan Selection.

H. Open Patient button removed (it didn't do anything anyway)

I. Patient selection textbox disable from editing.

J. Score Rx scaling disappears if PRIMARY plan Rx matches the scorecard

#### ScoreCard Builder improvements: Fixed issues with "Do Not Ask Again" on structure matching.
* Initial structure dictionary

![image](https://user-images.githubusercontent.com/78000769/169745487-1388db2e-346e-412b-b8c0-56751c0662fb.png)
* If a structure is unrecognized it will be unmatched.
![image](https://user-images.githubusercontent.com/78000769/169745557-ec7d929d-6493-4583-8d47-ec84177e1190.png)
* Then if you match a structure that is unmatched, you'll get the prompt about the structure dictionary.
![image](https://user-images.githubusercontent.com/78000769/169745606-b4c05044-f947-430a-8903-945876984094.png)
* If you say yes, you should see all the structures get matched. (just use the default) selection
![image](https://user-images.githubusercontent.com/78000769/169746037-52205861-2daf-4d4b-9f7d-243b26bd7e6c.png)
* Now the dictionary looks like this:

![image](https://user-images.githubusercontent.com/78000769/169746162-5eeffc0c-0332-40bd-a633-497c39193047.png)
* Note that you have to click on the new row to get the text in the structure column to update.
![image](https://user-images.githubusercontent.com/78000769/169746208-183c168a-e16b-45dd-bfde-bd9e1d673800.png)
* Then if you want to change that match you now can.
![image](https://user-images.githubusercontent.com/78000769/169746440-1b79c637-5b65-465d-b496-504decc5264f.png)
* The prompt won't come up again, but if you wanted to add this to the dictionary as well, then you can use the dictionary editor tool.
![image](https://user-images.githubusercontent.com/78000769/169746611-97aa32f2-1412-4f85-b6db-8f31b73998d7.png)
![image](https://user-images.githubusercontent.com/78000769/169746641-7c228fea-ccca-44fc-b66e-9890d2f6331a.png)
* Then you'll have 2 synonym entries in there so make sure that if you have 2 matches in your structure set, maybe you don't know which one the tool is going to choose (but I'd assume the first in order). Notice when I reload the tool they all match to "Hearts" because that's the first in the list.

![image](https://user-images.githubusercontent.com/78000769/169746989-26210997-7a36-49c4-bfa2-d2879e9a8211.png)
* If you wanted to modify the actual scorecard, there is a new button to do that. This should allow you to maybe change the scorecard to a new naming convention you'd want to use.
![image](https://user-images.githubusercontent.com/78000769/169747053-a6138210-3985-45e6-8d65-35c34d2f83bb.png)
* This will change the value in the scorecard (although even just changing the Plan structure and resaving would allow for you to reload the scorecard because the tool look at structure ID first and then tries to match from Template Structure Id second)
* Finally, there is a new tool for managing the scorecard. If you use the last option to Delete Match, you can break the complete between a structure and its structure checkbox.
![image](https://user-images.githubusercontent.com/78000769/169747190-9be0e8ac-8432-4fe3-bcc6-592a147a50c3.png)
* This removes the structure synonyms from that structure completely (because I selected both of the matches for removal)
![image](https://user-images.githubusercontent.com/78000769/169747248-02860552-d0f7-446e-9539-e6f5e2b162f5.png)
	 
#### Structure Builder improvements
  * Operations between non-HighRes and HighRes solved (non-HighRes converted to HighRes first)

### Bugfixes:
* Fixed issue where scorecard was crashing on modified gradient index.
  * This was crashing because the structure was left empty when building the scorecard. Now if the structure is null, then the scorecard will not crash, but going forward if the user tries to build a scorecard and doesn't select the structure than it will not let the user save.
  * Please choose body when selecting this metric if the metric is to be calculated in the entire body
* Fixed CI throwing error
* Fixed HI defaulting to cGy when not selected
* Set the structure collection to clear each time so it should only show the primary plan's structures.
* Can now delete metric comment all the way
  
### Known issues:
* Unable to change metric type or edit metrics after clicking recalculate score (must restart PlanScoreCard and reload to edit)
* No longer real time plot updating in Metric Editor
* Intermittent color/label change on adjacent color/label in qualitative label/edit color picker
* Certain patients cause crash on launching PlanScoreCard (only seen in V17)
* Reparse existing structurebuilder structures for editing (currently must delete and re-create)
* Modified gradient index crashes if values left blank
 
### 2.X Feature roadmap:
* guided structure builder (aka: simple mode, to be in front of current advanced structure builder)
* reload current patient from Eclipse button
* command line interface to output plan scores to the CLI or automate generation of CSV/PDF
* Import/Export buttons on ScoreCard Builder to support opening and saving various file types
* optional DVH rendering below UI with horizontal(D@V)+vertical(V@D) scoring brackets per selected structure

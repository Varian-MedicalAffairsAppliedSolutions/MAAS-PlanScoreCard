## Changelog
## V2.5.5.X-06/01/2022
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
* Main When Structure ID = null not automatching to TemplateStructureId
* ScoreCardBuilder Intermittent color/label change also changes lowest color/label in qualitative label/edit color picker
* ScoreCardBuilder Copying metric without structure assigned or color assigned crashes app
* ScoreCardBuilder Re-rank button reranks metrics based on score on the screen but the changes are not saved (either: to file / click off and back on that metric)
* StructureBuilder Reparse existing structurebuilder structures for editing (currently must delete and re-create)
 
### 2.X Feature roadmap:
* guided structure builder (aka: simple mode, to be in front of current advanced structure builder)
* reload current patient from Eclipse button
* command line interface to output plan scores to the CLI or automate generation of CSV/PDF
* Import/Export buttons on ScoreCard Builder to support opening and saving various file types
* optional DVH rendering below UI with horizontal(D@V)+vertical(V@D) scoring brackets per selected structure


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

## Changelog
## 3.1.4.X (1/10/2022)

### Improvements

* Lauuch speed up: only loads current plan in context automatically (instead of all plans and courses for patient)
* New seperate "open" button to load all plans and courses for patient
* Label now "Scorecard Structure ID" not "Template Structure ID" in editor
* ScoreCard stuctructure ID now used in multi-patient batch selection matching
* Multi-patient batch selection matching now 1:1 on stuctures not metrics (less redundant matching)
* Dictionary matching improved in multi-patient batch selection matching
* Multi-patient batch selection matching hover over tool tips added

### Bugfixes: 

* No longer crashes on launch for patients with portal dosimetry plans
* Scorecard editor now references correct attached structure set for primary loaded plan 
* Correceted "normailize to max score" bug displaying only negitive numbers and not relliably finding max score (introduced with tails on fail/0)
* Plan selection "Patient "Course" "Plan" values now correctly not editable (would cause a crash when attemtping edits directly)

### Known issues: 

* Dose precision is again no longer rounded to the nearest hundredth value when rescaling
* "remove selected patient" button in multi-patien batch selection crashes app
* Prevent single point metrics from being saved as a template or recalculated
* A crash occurs if the Rx dose is scaled too low and the conformation number metric is used
* When changing the metric type of an exisiting metric the input units don't always update to compatible selections
* Multi-patien batch selection window default size too small
* Multi-patien batch selection plan validation table closes whenever a new local match or addition to the structure dictionary is made
* Normalizing with a scorecard that has a HI metric will cause the application to crash 
* Adding a point before selecting a metric will cause the application to crash


## 3.0.3.X (11/08/2022)
### Improvements
* Batch mode: CSV input with multiple patients/plans and report output 
![image](https://user-images.githubusercontent.com/78000769/208266047-331154ea-946f-4e5f-9597-7f3a20297e93.png)
![image](https://user-images.githubusercontent.com/78000769/208266064-b38b04d9-f2d6-4797-8017-e54d4b691f6b.png)
* Patient ID added to scorecard warnings and flags 
![image](https://user-images.githubusercontent.com/78000769/208266072-672c2a4f-03d2-4552-b4d6-988bdfe5ded8.png)
* Implement point falloff logic (tails) once a metric fails (zero points) when normalizing to max score. Tail values can be adjusted in the “PlanScoreCard.exe” config file.
![image](https://user-images.githubusercontent.com/78000769/208266421-e0f8ca06-cb41-474a-9c87-ea2559cbc623.png)
![image](https://user-images.githubusercontent.com/78000769/208266434-d454fbb3-784c-45f3-9013-9d02149aaa41.png)


### Bugfixes: 

* Invalid null structures can no longer be added to the structure dictionary 
* All unselected plan scores are now cleared in the last column on the following events: rx scaling, scorecard editing, and loading a new scorecard 
* Volume precision is rounded to the nearest hundredth value when rescaling. 
* Rescaling now correctly applies to HI metric 
* Color selection option was changed from “Cancel” to “Clear” 

### Known issues: 

* Prevent single point metrics from being saved as a template or recalculated. 
* A crash occurs if the Rx dose is scaled too low and the conformation number metric is used. 
* Score values on Y-axis during plan normalization are incorrect and sometime tails are wrongly created: normalize to max score not 100% reliable
* Normalizing with a scorecard that has a HI metric will cause the application to crash 
* Adding a point before selecting a metric will cause the application to crash 

## 2.5.7.X (8/29/2022)
### Improvements: 
* If a structure is used in the structurebuilder that is not in the plan structure set, the user will receive a message indicating which structure(s) are missing

![image](https://user-images.githubusercontent.com/78000769/187831226-fd238085-544f-4d96-9d4b-5bf0d41ad227.png)

* Added a button to remove all colors from the scorepoints

![image](https://user-images.githubusercontent.com/78000769/187831258-5ec8574f-0385-44fa-b6ff-09cc07a0dd42.png)

* Added a point label to the color picker tool

![image](https://user-images.githubusercontent.com/78000769/187831520-13ff09d5-0aae-4351-8f9b-158da60c41de.png)

* Added the structure Id to the warnings and failures

![image](https://user-images.githubusercontent.com/78000769/187831333-807d6f56-1dd5-4455-99db-c404948745b6.png)

### Bugfixes: 
* Flag now resets on rescore so the warning and flag symbols will appear and disappear as needed 
* Structure matching will now use the template structure id if the structure id is null and template is not null 
* Removed the structure building from the normalization space. (The normalization button was not intended to build structures) 
* The normalized button is only made available when a scorecard and plan is selected 
* Normalizing to max score now uses the most updated scorecard to find the max value of the score (It was using the imported scorecard before) 
* Normalizing to max score now uses the normalization value with the max score that is closest to the initial plan normalization 
* Fixed metric copying crash when the color was null 
* The score labels now automatically update in the color blocks when edited. 
* Both the ranking in the score metric and in the json file have been fixed by applying changes occurring in the local class to the metric template (Fixed “Order Points By Value” button) 
* Copying a metric without a structure no longer causes a crash 
* HI metric now accepts doubles 
* Color picking tool now only changes the color that was selected when changing the color 
* Tooltip corrected for structure Id template update button 

### Known issues: 
* Prevent single point metrics from being saved as a template or recalculated. 
* A crash occurs if the Rx dose is scaled too low and the conformation number metric is used. 

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

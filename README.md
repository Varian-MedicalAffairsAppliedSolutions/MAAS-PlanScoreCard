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
* Example ScoreCards can be found here: https://medicalaffairs.varian.com/dose-scorecards

### Main Screen
![MainScreen1 0 0](https://user-images.githubusercontent.com/78000769/200408043-41a14e05-d9e3-4cad-99a0-442bf8e6a654.png)


### ScoreCard Editor
![ScoreCardEditor1 0 0](https://user-images.githubusercontent.com/78000769/169741156-a27d6165-616a-4181-80a7-74ed7923e8eb.png)

### Structure Builder
![StructureBuilder1 0 0](https://user-images.githubusercontent.com/78000769/169741212-82f16a0d-1e11-4ee4-863d-ac2446486c5b.png)

### Installation Guide: [Quick-Start](../master/BasicInstallQuickStart.md)

### Installation Guide Part2: [Move PlanScoreCard to system scripts directory, copy and edit launcher (optional)](../master/InstallGuidePart2IntoSystemScriptsDirectory.md)

### Errors when launching? See: [Troubleshooting](../master/Troubleshooting.md)

## Current version: 2.5.7.X (8/29/2022)
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

### 2.X Feature roadmap: 
* Batch mode: CSV input with multiple patients/plans and report output 
* Implement point falloff logic (tails) once a metric fails (zero points) when normalizing to max score 
* Guided structure builder (aka: simple mode, to be in front of current advanced structure builder) 
* Reload current patient from Eclipse button 
* Command line interface to output plan scores to the CLI or automate generation of CSV/PDF 
* DVH rendering popout UI with horizontal(D@V)+vertical(V@D) scoring brackets per selected structure  
* Import/Export buttons on ScoreCard Builder to support opening and saving various file types 
* Include support for asymmetric expansions in the advanced structure builder

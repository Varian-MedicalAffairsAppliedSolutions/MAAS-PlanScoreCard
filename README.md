# PlanScoreCard

##### Medical Affairs Applied Solutions ESAPI tool to create dosimetric ScoreCards and score plans.
##### Features:
* Quantitative piecewise linear scoring functions for each metric
  * optional: flag for point where "variation acceptable" sited on referenced protocol
  * optional: note section to site referenced protocol or justification for metric (points)
  * optional: qualitative colors and labels for metric points, ie: orange="Just OK"
* Advanced scoring criteria supported

  ![image](https://user-images.githubusercontent.com/78000769/208264370-51b853f4-59dd-498a-8dd7-17b093d0e6f2.png)
  
* Compare multiple plans in multiple courses at the same time for a given patient
* Compare plans on multiple patients simultaniously in BatchMode (NEW in v3.X)
  * save patient/plan batch list to .json file for subsequent re-evalution of a scorecard on that list
* Advanced structure matching logic with integrated customizable preloaded json structure dictionary
* In-metric Structure Boolean/expansions created dynamically for scoring 
  * optionally save back to Plan if script approval/writable (to auto-create optimization tuning structures)
* Normalize dose to max score
  * removes noise during plan comparison from suboptimal normalization impacting relative scores
  * re-normalized to max score plans saved into a new course
* DVH view to better visualize Dose@Volume & Volume@Dose and other selectable metrics (NEW in v3.X)
* Dose per fraction and number of fractions stored in scorecard
  * Linear scaling of all doses on scorecard possible if # fx and dose/fx not matching Eclipse
* Checkbox to annotate note explaining specific metric performance (for report printing)
* CSV or PDF output reports
* Example ScoreCards can be found here: https://medicalaffairs.varian.com/dose-scorecards

### Main Screen
![image](https://github.com/Varian-MedicalAffairsAppliedSolutions/MAAS-PlanScoreCard/assets/78000769/23bcb6c8-2047-4955-8dd8-3af3e2fc6b21)
1)	Checks ScoreCard Rx matches Rx in Eclipse: On Rx mismatch, option to scale all doses by % difference
2)	Open previously saved ScoreCard and score currently selected plan(s)
3)	Create new ScoreCard or edit currently loaded ScoreCard
4)	Open additional patients/plan(s) to score in a batch (menu shown below)
5)	Open all courses and plans for selected patient
6)	Normalize dose to maximum score 
a)	Default: normalize all selected plans
b)	Change in configuration to normalize primary plan only (see 12)
7)	Export values in current ScoreCard evaluation view to CSV (Pandas Dataframe)
8)	Render current ScoreCard evaluation view to PDF for printing/reporting
9)	Solid selected box indicates primary plan selected (bold crosshair on plot)
10)	Warnings and flags 
a)	Red – Metric failing
b)	Yellow – Metric in variation range
c)	Purple – Structure missing
11)	 Indication when a plan structure has been matched to a scorecard structure
12)	 Configuration page (options shown below)
13)	 Open DVH viewer (shown below)


### ScoreCard Editor
![image](https://github.com/Varian-MedicalAffairsAppliedSolutions/MAAS-PlanScoreCard/assets/78000769/f41a1357-efc1-481c-b863-cb87d0e63991)
1)	Add a new metric to score
2)	Delete selected score metric
3)	Copy selected score metric
4)	Opens structure builder (details below)
5)	Moves rank number (up or down) of selected metric
6)	Add a new point to selected score metric
7)	Delete selected score point on selected score metric
8)	Order metric points in order of score value (highest to lowest)
9)	Clears colors and labels from all metric points
10)	 Moves rank number (up or down) of the selected metric point
11)	Pushes plan structure ID to scorecard structure ID for selected metric
12)	Opens structure dictionary (details below)
13)	Select metric point that begins “variation acceptable” range (yellow line) of the piecewise linear function
14)	Add colors and labels for each metric point
15)	Select treatment site (options can be edited in config file)
16)	Recalculates plan score and returns to main screen
17)	Save changes to scorecard

### Simple Structure Builder
![image](https://github.com/Varian-MedicalAffairsAppliedSolutions/MAAS-PlanScoreCard/assets/78000769/37df3f06-a259-47aa-8e67-853026e7a581)
1)	Add a new structure to first structure grouping
2)	Symmetric margin from selected structure
3)	Asymmetric margin menu for selected structure
4)	Delete operation step
5)	Select Boolean operator
6)	Options and selections for second structure grouping
7)	Expanded menu showing all operations
8)	Free text field for naming generated structure
9)	Creates plan structure in metric editor. Note that the structure is not generated until assigned to a score metric in the Scorecard editor.


### Multi-Plan/Multi-Patient Selection Workspace
![image](https://github.com/Varian-MedicalAffairsAppliedSolutions/MAAS-PlanScoreCard/assets/78000769/4381375b-f9a1-4dae-9e9b-98554f5154e3)
1)	Import saved patient selection JSON file
2)	Search for patient name in database
3)	Add selected patient from database
4)	List of patients added
5)	List of plans within highlighted patient in selected patients list
6)	Alerts for structure matching with scorecard metrics
7)	Validate structure matches with scorecard for each plan
8)	List of structure names used for scorecard metrics
9)	List of structures within plan structure set
10)	Checkmarks indicate how structures are matched (direct, locally, or via dictionary)
11)	Drop down menu for choosing structure within plan
12)	Assign local structure match (plan specific)
13)	Add match to structure dictionary (global)
14)	Score selected patients and plans
15)	Save patient selection as JSON file
16)	Remove selected patient from list
17)	Close patient selection workspace


### Configuration Page Options
![image](https://github.com/Varian-MedicalAffairsAppliedSolutions/MAAS-PlanScoreCard/assets/78000769/18a7fd6d-45a9-4e07-9ce5-697ecde381d8)


### Structure Dictionary
![image](https://github.com/Varian-MedicalAffairsAppliedSolutions/MAAS-PlanScoreCard/assets/78000769/dc73eb67-eca4-4c5c-9a0e-1f9bef2564c5)


### DVH View
![image](https://user-images.githubusercontent.com/78000769/229590966-85f77072-5f55-4d25-89d6-4934557ea475.png)

### Installation Guide: [Quick-Start](../master/BasicInstallQuickStart.md)

### Installation Guide Part2: [Move PlanScoreCard to system scripts directory, copy and edit launcher (optional)](../master/InstallGuidePart2IntoSystemScriptsDirectory.md)

### Errors when launching? See: [Troubleshooting](../master/Troubleshooting.md)

### Still have questions? See: [FrequentlyAskedQuestions](../master/FAQ.md)

## Current version: 3.2.0.X (6/18/2024)

### Improvements

* DVH rendering popout UI with horizontal(D@V)+vertical(V@D) scoring brackets per selected structure
* User can now select multiple metrics using CTRL/Shift in the editor 
* Normalization to max score range is now configurable and “index” type metrics can be turned off during normalization 
* Negative scores are no longer plotted by default for normalization unless PlotNegative is “true” in config file

### Bugfixes: 

•	Normalizing with a modified gradient metric no longer causes score discrepancies 
•	Fixed normalizing to max score plotting view
•	Single point metrics are now highlighted in red to prevent being saved as a template or recalculated
•	Fixed issue for new structure dictionary entry, the user had to click outside of the “Key” text box for the “OK” button to be enable
•	Fixed crash with asymmetric margin tool
•	Fixed issue with scorecard structure ID saving
•	Issue fixed with incorrect structure matching from dictionary
•	Fixed bug where plan scores were not displayed

### Known issues: 

* Better Lables on Simple structure buider

### For changes in previous versions see: [ChangeLog](../master/ChangeLog.md)

### 3.X Feature roadmap: 
* BatchMode re-optimize multiple plans with RapidPlan model
* * Add stylized html report output option (in addtion to clean CSV and viewdump PDF)
* DICOM Filemode support to be enabled by importing opensource C# DICOM library
*	Import/Export buttons on ScoreCard Builder to support opening and saving various file types
*	Reload current patient from Eclipse button
~~*	BatchMode normalize multiple plans to max score~~ DONE
~~*	Guided structure builder (aka: simple mode, to be in front of current advanced structure builder)~~ DONE
~~*	Include support for asymmetric expansions in the structure builder~~ DONE
~~*	Command line interface to output plan scores to the CLI or automate generation of CSV/PDF~~ POSSIBLE WITH API 
~~* API interface~~ DONE


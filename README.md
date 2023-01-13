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
* Dose per fraction and number of fractions stored in scorecard
  * Linear scaling of all doses on scorecard possible if # fx and dose/fx not matching Eclipse
* Checkbox to annotate note explaining specific metric performance (for report printing)
* CSV or PDF output reports
* Example ScoreCards can be found here: https://medicalaffairs.varian.com/dose-scorecards

### Main Screen
![image](https://user-images.githubusercontent.com/78000769/210690044-26142a34-6109-4b75-9abf-467ac44f4089.png)
1.	Checks ScoreCard Rx matches Rx in Eclipse: On Rx mismatch, option to scale all doses by the % difference
2.	Open previously saved ScoreCard and score currently selected plan(s)
3.	Create new ScoreCard or edit currently loaded ScoreCard
4.	Open additional patients/plan to additionally score in batch (NEW in v3.X)
5.	Open all courses and plans for selected patient
6.	Normalize dose in primary selected plan to maximum score
7.	Dump values in current ScoreCard evaluation view to CSV (Pandas Dataframe)
8.	Render current ScoreCard evaluation view to PDF for printing/reporting
9.	Solid selected box indicates primary plan selected
10.	Warnings and flags when a metric is below variation or scores 0 points
11.	Indication that a plan structure has been matched to a scorecard structure

### ScoreCard Editor
![image](https://user-images.githubusercontent.com/78000769/210690412-a6f01bfd-5975-4008-ac33-5bf57c9dcecb.png)
1.	Add a new metric to score
2.	Delete selected score metric
3.	Copy selected score metric
4.	Opens structure builder
5.	Moves the rank number (up or down) of selected metric
6.	Add a new point to the selected score metric
7.	Delete selected score point on selected score metric
8.	Order metric points in order of score value (highest to lowest)
9.	Clears colors and labels from all metric points
10.	 Moves the rank number (up or down) of the selected metric point
11.	Pushes the plan structure ID to the scorecard structure ID for the selected metric
12.	Opens the structure dictionary
13.	Select metric point that begins the “variation acceptable” range (yellow line) of the piecewise linear function
14.	Add colors and labels for each metric point
15.	Select treatment site (options can be edited in config file)
16.	Recalculates the plan score and returns to the main screen
17.	Save changes to json file

### Structure Builder
![image](https://user-images.githubusercontent.com/78000769/210690982-01ea862b-328b-4a80-a538-42d39de1d89e.png)
1.	Add a new structure grouping
2.	Add an operation step to the selected structure group
3.	Delete operation step
4.	Edit the operation steps of a selected group
5.	Select the Boolean operation between two intragroup steps
6.	Delete the selected group (along with all its operations)
7.	Choose the Boolean operation between different groups
8.	Demote/promote the selected group order sequence
9.	Free text field for the generated structure name
10.	Creates the plan structure in the metric editor. Note: The structure is not generated until it is assigned to a score metric in the Scorecard editor and a plan is scored with that scorecard.

### Installation Guide: [Quick-Start](../master/BasicInstallQuickStart.md)

### Installation Guide Part2: [Move PlanScoreCard to system scripts directory, copy and edit launcher (optional)](../master/InstallGuidePart2IntoSystemScriptsDirectory.md)

### Errors when launching? See: [Troubleshooting](../master/Troubleshooting.md)

### Still have questions? See: [FrequentlyAskedQuestions](../master/FAQ.md)

## Current version: 3.1.4.X (1/10/2022)

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

### For changes in previous versions see: [ChangeLog](../master/ChangeLog.md)

### 3.X Feature roadmap: 
* BatchMode re-optimize multiple plans with RapidPlan model
*	BatchMode normalize multiple plans to max score
* DVH view button for popout UI with horizontal(D@V)+vertical(V@D) scoring brackets per selected structure
*	Guided structure builder (aka: simple mode, to be in front of current advanced structure builder)
*	Include support for asymmetric expansions in the advanced structure builder
*	Command line interface to output plan scores to the CLI or automate generation of CSV/PDF
* DICOM Filemode support to be enabled by importing opensource C# DICOM library
*	Import/Export buttons on ScoreCard Builder to support opening and saving various file types
*	Reload current patient from Eclipse button
* API interface


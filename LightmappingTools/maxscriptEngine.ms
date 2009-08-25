global preserveMaterials= true
global preserveLights=true
global wasCancelled=false
global hasBeenImportedOnce=false
global notFound=false
global showVFB=true
global presetArray=#()
global resArray=#()
global fileFormat=".dds"
global SaveDir=""
global presetDir = ""
global presetFile=""
global matLibFile=""
global imported_array=#()
global sceneName=""
global conversorPath=""

fn renderLightmap obj currentMapType Xsize Ysize channel fullFileName =
(
	print fullFileName
	bi = obj.INodeBakeProperties
	currentMapType.outputSzX=Xsize
	currentMapType.outputSzY=Ysize
	currentMapType.filenameUnique=true
	currentMapType.filename=fullFileName
	currentMapType.fileType=fullFileName
	print "works also here"
	bi.removeAllBakeElements()	
	bi.addBakeElement currentMapType
	bi.bakeChannel=channel
	bi.bakeEnabled = true
	render rendertype:#bakeSelected outputwidth:Xsize outputheight:Ysize cancelled:&wasCancelled progressbar: true vfb: showVFB
	format "rendering node:% channel:%\n" obj.name bi.bakeChannel
)

fn BeforeSave=
(
	for i in imported_array do
	(
		if (imported_array.count>0) then
		(
			if (preserveMaterials) then 
			(
				toRemove =#()
				for i in currentMaterialLibrary do (append toRemove i.name)
				for i in toRemove do (deleteItem currentMaterialLibrary(i))
			)
			for i in imported_array do
			(
				if (preserveMaterials) then 
				(
					try
					(
						append currentMaterialLibrary i.material
						print "rzucil do bibl"
					)
					catch()
				)
				try(
					print (superclassof i)
					if (preserveLights and ((superclassof i)==light)) then
					(
						i.name = uniquename "LightFromUnity"
					)
					else
					(
						delete i
					)
				)
				catch()
			)
			imported_array=#()
		)
	)
)

fn assignMatLib=
(	
	if (selection.count==0) then
	(
		max select all	
	)
	
	if (preserveMaterials) then
	(
	    local notFound=true
	    for i in selection where (superclassof i)==geometryclass do
	    (
print "1"
		    if (i.material!=undefined) then
		    (
print "2"
     	    try
			    (
print "3"
				    for j=1 to i.material.count do
				    (
	                    notFound=true
print "4"
					    if (i.material[j]!=undefined) then
					    (
print "5"
						    for z in currentMaterialLibrary while notFound do
                         (
print "6"
							    try
							    (
print "7"
								    for y in z while notFound do
								    (
print "8"
									    if(y!=undefined) then
									    (
print "9"
										    if (y.name==i.material[j].name) then
										    (
											    i.material[j]=y
print "multi-multi"
											    notFound=false
										    )
									    )
								    )
							    )
							    catch
							    (	
print "10"
								    if(z!=undefined) then
								    (
									    if (z.name==i.material[j].name) then
									    (
										    i.material[j]=z
print "single-multi"
										    notFound=false
                                     )
								    )
							    )
						    )
					    )
					    --if the material from object multimaterial list is undefined then it got no name
				    )
			    )
			    catch
			    (
print "11"
				    notFound=true
				    for z in currentMaterialLibrary while notFound do(
					    try
					    (
print "12"
						    for y in z while notFound do
						    (
print "13"
							    if(y!=undefined) then
							    (
								    if (y.name==i.material.name) then
								    (
									    i.material=y
print "multi-single"
									    notFound=false
								    )
							    )
						    )
                     )
					    catch
					    (
print "14"
						    if(z!=undefined) then
						    (
print "15"
							    if (z.name==i.material.name) then
							    (
							 	    i.material=z
print "single-single"
								    notFound=false
							    )
						    )
					    )
				    )
			    )
		    )
	    )
	)
)

fn AfterSave=
(	
	max select all
	local temp = selection as array
	try
	(
		FbxImporterSetParam "Mode" "merge"
	)
	catch(
		print "problem with fbx options mode merge"
	)
	try
	(
		if (preserveLights) then
		(
			FbxImporterSetParam "Lights" false
		)
		else
		(
			FbxImporterSetParam "Lights" true
		)
	)
	catch(
		print "problem with fbx options Lights"
	)
	
	importFile (presetDir + sceneName + ".fbx") #noPrompt using:FBXIMP
	max select all
	for i in temp do
	(
		deselect i
	)
	imported_array = selection as array
	assignMatLib()
	setSaveRequired false
)


fn BakeObjects=
(
	max select none
	SetSelectFilter 2
	max select all
	setSelectFilter 1
	progressStart "Baking textures..."
	j=0
	for i in (selection as array) do
	(
		print i.name
		if ((findItem imported_array i)!=0) then
		(
			print (i.name + " consisted")
			if (not getProgressCancel()) then 
			(
				select i
				try
				(
					local alightMap
					if (((findString (renderers.current as String) "V_Ray")!=undefined) or ((findString (renderers.current as String) "VRay")!=undefined)) then 
					(
						print "using Vray Lightning Map to bake object " + (j as string)
						try
						(
							alightMap=VRay_RawTotalLightingMap()
						)
						catch
						(
							alightMap=VRayRawTotalLightingMap()
						)
					)
					else
					(
						alightMap =LightingMap()
					)
					print "works till here"
					destination =(SaveDir + "lightmap" + (j as string)+fileFormat)
					renderLightmap i alightMap resArray[j+1] resArray[j+1] 3 destination
				)
				catch print "There was a problem rendering lightmap"
			)
			j=j+1
		)
		
		percent= (100.0*j)/selection.count
		progressUpdate percent
	)
	progressEnd()
	if (not wasCancelled) then
	(
		messagebox ("End of the rendering process!" )
	)
	else
	(
		messagebox ("Rendering canceled!" )
	)
)

fn buildPresetsArray=
(
	presetArray = getFiles (presetDir + sceneName + "_*.max")
)


fn ImportScene=
(	
	if (presetFile!="") then
	(
		try
		(
			saveMaxFile presetFile useNewFile:true
		)
		catch
		(
			print "Major error"
		)
	)
	else
	(
		try
		(
			createDialog inputBox
		)
		catch
		(
			print "Cannot start a new preset"
		)
	)
)


rollout inputBox "Name for a new preset" width:200 height:40 
(
	editText inputTxt "" pos:[10,10] width:180
	
	on inputTxt entered txt do
	(
		if (txt != "") then
		(
			presetFile=(presetDir+sceneName+"_"+txt +".max")
			ImportScene()
			--saveMaxFile presetFile useNewFile:true
		)
		else
		(
			print "you should input a preset name"
		)
		buildPresetsArray()
		
		destroyDialog(inputBox)
	)
)



--GUI
utility MyUtil "Lightmapping Tool"
(	

	checkbox preserveMat "Preserve materials" checked:preserveMaterials
	on preserveMat changed statee do preserveMaterials=statee
	
	checkbox preserveLht "Preserve Unity lights" checked:preserveLights
	on preserveLht changed statee do preserveLights=statee
	
	button btnImport "Reimport Unity scene" width:144
	
	
	checkbox showFrame "Show VFB" checked:preserveMaterials
	on showFrame changed stateee do showVFB=stateee
		
	button btnBake "Bake now!" width:144
	on btnBake pressed do
	(
		BakeObjects()
	)
	
	button btnSwStandard "Change Materials type" width:144
	on  btnSwStandard pressed do
	(
		fileIn conversorPath
	)
	
		listbox presetsList "Max scene presets:" items:(for o in (presetArray=getFiles (presetDir + sceneName + "_*.max")) collect ((getFilenameFile (o as string)) + "   " + (getFileModDate (o as string))) )
		button btnCreatePreset "Create new preset" width:144
		button btnRemovePreset "Remove selected" width:144
		button btnRefreshPreset "Refresh list" width:144
		
		fn refreshList=
		(
			for i = 1 to (presetsList.items.count) do
			(
				if (presetsList.items.count) != 0 then
				(
					(presetsList.items) =deleteItem (presetsList.items) 1
				)
			)
			presetArray = (getFiles (presetDir + sceneName + "_*.max"))
			for o in (getFiles (presetDir + sceneName + "_*.max")) do
			(
				presetsList.items=append(presetsList.items) ((getFilenameFile (o as string)) + "   " + (getFileModDate (o as string)) )
			)
		
		
		)
		
		on btnRefreshPreset pressed do
		(	
			refreshList()
		)
		
		on presetsList doubleClicked itm do
		(
		
				if (matLibFile!="") then
				(
					saveMaterialLibrary matLibFile
				)
				loadMaxFile presetArray[itm]
				presetFile= presetArray[itm]
				
				max select all
				local temp = selection as array
				try
				(
					FbxImporterSetParam "Mode" "merge"
				)
				catch(
					print "problem with fbx options mode merge"
				)
				try
				(
					FbxImporterSetParam "Lights" true
				)
				catch(
					print "problem with fbx options Lights"
				)
				
				importFile (presetDir + sceneName + ".fbx") #noPrompt using:FBXIMP
				max select all
				for i in temp do
				(
					deselect i
				)
				imported_array = selection as array
				assignMatLib()
				refreshList()

		)
		
		
		on btnImport pressed do
		(
			if (matLibFile=="" and (queryBox "Start a new MatLib file? No - to load the old one if exists")) then
			(
				matLibFile=presetDir+sceneName+"_matlib.mat"
				saveMaterialLibrary matLibFile
			)
			else
			(
				try
				(
					matLibFile=presetDir+sceneName+"_matlib.mat"
					loadMaterialLibrary matLibFile
					assignMatLib()
				)
				catch
				(
					saveMaterialLibrary matLibFile
				)
			)
			try
			(
				fileIn(presetDir + sceneName + ".ms")
			)
			catch
			(
				messagebox("Can't load config file")
			)
			if (matLibFile!="") then
			(
				saveMaterialLibrary matLibFile
			)
			refreshList()
		)
		on btnCreatePreset pressed do
		(	
			createDialog inputBox
			refreshList()
		)
		on btnRemovePreset pressed do
		(
			if (presetsList.items.count>0 and presetsList.selection>0) then
			(
				try
				(	
					deleteFile presetArray[presetsList.selection]
				)
				catch
				(
					messagebox("I couldn't delete the preset - try deleting it manually")
				)
				refreshList()
			)
		)
		
		on MyUtil open do
		(
			try
			(
				refreshList()
			)
			catch()
		) 
)

fn assignRenderer=
(
	rIndex=-1
	for i=1 to RendererClass.classes.count while (rIndex==-1) do if ((findString (RendererClass.classes[i] as string) "V_Ray")!=undefined) then rIndex=i
	if(rIndex!=-1) then(
	renderers.current=RendererClass.classes[rIndex]()
	print "Vray assigned"
	)
)

fn assignMaterials=
(
	searchFor = #("","Scanline","mental","V_Ray","VRay","Final","Brazil","Maxwell")
	currRenderer=-1
	for i=1 to searchFor.count while (currRenderer==-1) do (if ((findString (renderers.current as string) searchFor[i])!=undefined) then currRenderer=i)
	if (currRenderer==3 and (queryBox "Switch to Mental Ray Materials?")) then fileIn conversorPath
	else if ((currRenderer==4 or  currRenderer==5 ) and (queryBox "Switch to VRay Materials?")) then fileIn conversorPath
)


buildPresetsArray()
autoBackup.enabled = false
--myUtil.open
callbacks.addScript #filePreSave "BeforeSave()"
callbacks.addScript #filePostSave "AfterSave()"
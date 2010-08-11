global preserveLights=false
global wasCancelled=false
global notFound=false
global showVFB=true
global resArray=#()
global fileFormat=".tif"
global SaveDir=""
global presetDir = ""
global presetFile=""
global matLibFile=""
global imported_array=#()
global sceneName=""
global conversorPath=""
global modif=""

fn checkFBXVersion=
(
	try
	(
		fileVersion = GetFileVersion (getDir #maxRoot + "\\stdplugs\\fbxmax.dlu")
		arrry = filterString fileVersion ","
		
		if (((arrry[1] as number) == 2009) and  ((arrry[2] as number) == 4)) then
		(
			
		)
		else if (((arrry[1] as number) < 2010)) then
		(
			messagebox("You're using an old version of the 3dsmax FBX Importer - which can make the Lightmapping system work incorrectly. Please download the newest one from the Autodesk official website")
			try
			(
				shellLaunch "iexplore.exe" "http://usa.autodesk.com/adsk/servlet/pc/item?siteID=123112&id=10775855"
			)
			catch()
		)
	
	)
	catch
	(
		messagebox("You don't have fbx importer installed in your system, please download one from autodesk")
		try
		(
			shellLaunch "iexplore.exe" "http://usa.autodesk.com/adsk/servlet/item?siteID=123112&id=10775855"
		)
		catch()
	)
)
checkFBXVersion()


fn renderLightmap obj currentMapType Xsize Ysize channel fullFileName =
(
	print fullFileName
	bi = obj.INodeBakeProperties
	currentMapType.outputSzX=Xsize
	currentMapType.outputSzY=Ysize
	currentMapType.filenameUnique=true
	currentMapType.filename=fullFileName
	currentMapType.fileType=fullFileName
	bi.removeAllBakeElements()
	bi.addBakeElement currentMapType
	bi.bakeChannel=channel
	bi.bakeEnabled = true
	bi.nDilations = 4
	render rendertype:#bakeSelected outputwidth:Xsize outputheight:Ysize cancelled:&wasCancelled progressbar: true vfb: showVFB
	format "rendering node:% channel:%\n" obj.name bi.bakeChannel
)

fn fileExists fname = (getfiles fname).count != 0

fn BakeObjects=
(
	wasCancelled = false
	progressStart "Baking textures..."
	
	local alightMap
	if (((findString (renderers.current as String) "V_Ray")!=undefined) or ((findString (renderers.current as String) "VRay")!=undefined)) then 
	(
		try
		(
			alightMap=VRay_RawTotalLightingMap()
			print "Using VRay_RawTotalLightingMap"
		)
		catch
		(
		if ((findString (renderers.current as String) "V_Ray_Adv_1_50_SP4")!=undefined) then
			(
				alightMap=VRayTotalLightingMap()
				print "Using VRayTotalLightingMap"
			)
			else
			(
				alightMap=VRayRawTotalLightingMap()
				print "Using VRayRawTotalLightingMap"
			)
		)
		
	)
	else
	(
		alightMap =LightingMap()
	)
	for i in Geometry do
	(
		if ((findString i.name "ImportedObj")!=undefined and (not wasCancelled)) then
		(
			j = (substring i.name 15 10) as integer
			if (not getProgressCancel()) then 
			(
				select i
				try
				(
					destination =(SaveDir + "lightmap" + (j as string)+fileFormat)
					renderLightmap i alightMap resArray[j] resArray[j] 3 destination
				)
				catch print ("There was a problem rendering lightmap of object " + i.name)
			)
		)
		--percent= (100.0*j)/Geometry.count
		--progressUpdate percent
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


fn BakeSelected=
(
	progressStart "Baking textures..."
	
	local alightMap
	if (((findString (renderers.current as String) "V_Ray")!=undefined) or ((findString (renderers.current as String) "VRay")!=undefined)) then 
	(
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
	
	for i in (Selection as array) do
	(
		if ((findString i.name "ImportedObj")!=undefined and (not wasCancelled)) then
		(
			j = (substring i.name 15 10) as integer
			if (not getProgressCancel()) then 
			(
				select i
				try
				(
					destination =(SaveDir + "lightmap" + (j as string)+fileFormat)
					renderLightmap i alightMap resArray[j] resArray[j] 3 destination
				)
				catch print ("There was a problem rendering lightmap of object " + i.name)
			)
			j=j+1
		)
		
		--percent= (100.0*j)/Geometry.count
		--progressUpdate percent
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


fn assignMaterials=
(	
	for i in Geometry do
    (
		if ((findString i.name "ImportedObj")!=undefined) then
		(
			if (i.material!=undefined) then
			(
 				try
				(
					for j=1 to i.material.count do
					(
						notFound=true
						if (i.material[j]!=undefined) then
						(
							for z in currentMaterialLibrary while notFound do
							(
								try
								(
									for y in z while notFound do
									(
										if(y!=undefined) then
										(
											if (y.name==i.material[j].name) then
											(
												i.material[j]=y
												notFound=false
											)
										)
									)
								)
								catch
								(	
									if(z!=undefined) then
									(
										if (z.name==i.material[j].name) then
										(
											i.material[j]=z
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
					notFound=true
					for z in currentMaterialLibrary while notFound do(
						try
						(
							for y in z while notFound do
							(
								if(y!=undefined) then
								(
									if (y.name==i.material.name) then
									(
										i.material=y
										notFound=false
									)
								)
							)
					 )
						catch
						(
							if(z!=undefined) then
							(
								if (z.name==i.material.name) then
								(
						 			i.material=z
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


fn assignRenderer=
(
	rIndex=-1
	for i=1 to RendererClass.classes.count while (rIndex==-1) do (if (((findString (RendererClass.classes[i] as string) "V_Ray")!=undefined) or ((findString (RendererClass.classes[i] as string) "VRay")!=undefined)) then rIndex=i)
	if(rIndex!=-1) then(
		renderers.current=RendererClass.classes[rIndex]()
		messagebox("VRay renderer assigned!")
	)
	else(
		messagebox("VRay renderer was not found, try assigning it manually")
	)
)


fn modifTime=
(
	getFileModDate (presetDir + sceneName + ".ms")
)

--GUI
rollout mainWindow "Unity Lightmapping Tool"
(
	bitmap the_bmp fileName:"logo2.png"
	
	timer watchout inverval:1000 active:true

	on watchout tick do
	(
		if (modif=="") then(modif=modifTime())
	
		else if ((modif as string)!=(modifTime() as string)) then
		(
			modif=(modifTime() as string)
			try
			(
				fileIn(presetDir + sceneName + ".ms")
			)
			catch
			(
				messagebox("Can't load config file")
			)	
		)
	)
	
	button btnBake "Bake all now!" width:144 height:30
	on btnBake pressed do
	(
		BakeObjects()
	)
	button btnBakeSelected "Bake selected now!" width:144
	on btnBakeSelected pressed do
	(
		BakeSelected()
	)	
	
	
	group "Advanced"
	(
		button btnReimport "Reimport scene manually" width:144
		button btnRevert "Revert to Unity materials" width:144
		button btnAssignRenderer "Assign VRay renderer" width:144
		button btnSwStandard "Batch Convert Materials" width:144	
		
	)
		
	on btnReimport pressed do
	(
		try
		(
			fileIn(presetDir + sceneName + ".ms")
		)
		catch
		(
			messagebox("Can't load config file")
		)
	)


	on  btnRevert pressed do
	(
		try
		(
			toRemove=#()
			for i in Geometry do
			(
				if ((findString i.name "ImportedObj")!=undefined) then
				(
					append toRemove i
				)
			) 
			for i in toRemove do
			(
				delete i
			)
			
			try
			(
				FbxImporterSetParam "Mode" "merge"
			)
			catch(
				print "There was a problem with setting 'merge' option in the FBX importer. Try downloading a newer version."
			)
			try
			(
				FbxImporterSetParam "SmoothingGroups" false
			)
			catch(
				print "There was a problem with setting of the re-evalute normals."
			)
			importFile (presetDir + sceneName + ".fbx") #noPrompt using:FBXIMP
		)
		catch
		(
			messagebox("Can't reimport the file. Make sure that the scene was exported at least once using Unity")
		)
	)
	
	on  btnAssignRenderer pressed do
	(
		assignRenderer()
	)
	on  btnSwStandard pressed do
	(
		fileIn conversorPath
	)
)



utility MyUtil "Unity Lightmapping Tool"
(	

	on MyUtil open do
	(
		try
		(
			cui.UnRegisterDialogBar mainWindow
		)
		catch()
		try
		(	
			DestroyDialog mainWindow
		)
		catch()
		CreateDialog mainWindow escapeEnable:false
		cui.RegisterDialogBar mainWindow
	) 
)

CreateDialog mainWindow escapeEnable:false
cui.RegisterDialogBar mainWindow
cui.DockDialogBar mainWindow #cui_dock_left
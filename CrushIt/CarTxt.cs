﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CrushIt
{
    public enum ImpactSpecClauseSystemPart
    {
        driver,
        engine,
        lf_brake,
        lf_wheel,
        lr_brake,
        lr_wheel,
        rf_brake,
        rf_wheel,
        rr_brake,
        rr_wheel,
        steering,
        transmission
    }

    public enum LollipopMode
    {
        NotALollipop,
        xlollipop,
        ylollipop,
        zlollipop
    }

    public enum GrooveMode
    {
        constant,
        distance
    }

    public enum GroovePathNames
    {
        None,
        straight,
        circular
    }

    public enum GroovePathMode
    {
        None,
        linear,
        harmonic,
        flash,
        controlled,
        absolute,
        continuous
    }

    public enum GrooveAnimation
    {
        None,
        rock,
        shear,
        spin,
        throb
    }

    public enum GrooveAnimationAxis
    {
        x,
        y,
        z
    }

    public enum FunkMode
    {
        constant,
        distance,
        lastlap,
        otherlaps
    }

    public enum FunkMatrixMode
    {
        None,
        rock,
        roll,
        slither,
        spin,
        throb
    }

    public enum FunkAnimationType
    {
        None,
        frames,
        flic
    }

    public class Car
    {
        string name;
        br_vector3 driversHeadOffset;
        br_vector2 driversHeadTurnAngles;
        br_vector4 mirrorCamera;
        string[] pratcamBorders;
        int[] engineNoises;
        bool stealWorthy;
        ImpactSpec impactTop;
        ImpactSpec impactBottom;
        ImpactSpec impactLeft;
        ImpactSpec impactRight;
        ImpactSpec impactFront;
        ImpactSpec impactBack;
        string[] gridImages;
        List<string>[] pixelmaps;
        List<string>[] materials;
        List<string> models;
        List<string> actors;
        List<int> actorLOD;
        string reflectiveScreenMaterial;
        List<int> steerableWheels;
        int[] leftFrontSuspension;
        int[] rightFrontSuspension;
        int[] leftRearSuspension;
        int[] rightRearSuspension;
        int[] drivenWheels;
        int[] nonDrivenWheels;
        float drivenWheelDiameter;
        float nonDrivenWheelDiameter;
        List<Funk> funks;
        List<Groove> grooves;
        List<br_crush> crushes;
        br_vector3 lrWheelPos;
        br_vector3 rrWheelPos;
        br_vector3 lfWheelPos;
        br_vector3 rfWheelPos;
        br_vector3 centreOfMass;
        List<br_bounds> boundingBoxes;
        List<br_vector3> additionalPoints;
        float turningCircleRadius;
        br_vector2 suspensionGive; // Forward, Back
        float rideHeight;
        float dampingFactor;
        float massInTonnes;
        float fReduction;
        br_vector2 frictionAngle;
        br_vector3 widthHeightLength;
        float tractionFractionalMultiplier;
        float downforceSpeed;
        float brakeMultiplier;
        float increaseInBrakesPerSecond;
        br_vector2 rollingResistance; // Front, Back
        int numberOfGears;
        int redLineSpeed;
        float accelerationInHighestGear;
        List<string> shrapnel;
        List<int> firePoints;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public List<string>[] Pixelmaps
        {
            get => pixelmaps;
            set => pixelmaps = value;
        }

        public List<string>[] Materials
        {
            get => materials;
            set => materials = value;
        }

        public List<string> Models
        {
            get => models;
            set => models = value;
        }

        public List<string> Actors
        {
            get => actors;
            set => actors = value;
        }

        public List<int> ActorLODs
        {
            get => actorLOD;
            set => actorLOD = value;
        }

        public List<br_crush> Crushes
        {
            get => crushes;
            set => crushes = value;
        }

        public Car()
        {
            pixelmaps = new List<string>[3];
            materials = new List<string>[3];
            models = new List<string>();
            actors = new List<string>();
            actorLOD = new List<int>();
            steerableWheels = new List<int>();
            funks = new List<Funk>();
            grooves = new List<Groove>();
            crushes = new List<br_crush>();
            boundingBoxes = new List<br_bounds>();
            shrapnel = new List<string>();
            additionalPoints = new List<br_vector3>();
            firePoints = new List<int>();

            for (int i = 0; i < 3; i++)
            {
                pixelmaps[i] = new List<string>();
                materials[i] = new List<string>();
            }
        }

        public static Car Load(string path)
        {
            DocumentParser file = new DocumentParser(path);
            Car car = new Car() { name = file.ReadLine() };

            if (string.Compare(Path.GetFileName(path).ToUpper(), car.name) != 0)
            {
                //Logger.LogToFile(Logger.LogLevel.Error, "Not a valid Carmageddon car .txt file, expected {0} but found {1}", Path.GetFileName(path).ToUpper(), car.name);
                return null;
            }

            if (file.ReadLine() != "START OF DRIVABLE STUFF")
            {
                //Logger.LogToFile(Logger.LogLevel.Error, "Expected \"{0}\", didn't get it.  Are you sure this is a Car.TXT file?", "START OF DRIVABLE STUFF");
                return null;
            }

            car.driversHeadOffset = file.ReadVector3();
            car.driversHeadTurnAngles = file.ReadVector2();
            car.mirrorCamera = file.ReadVector4();
            car.pratcamBorders = file.ReadStrings();

            if (file.ReadLine() != "END OF DRIVABLE STUFF")
            {
                //Logger.LogToFile(Logger.LogLevel.Error, "Expected \"{0}\", didn't get it.  Are you sure this is a Car.TXT file?", "END OF DRIVABLE STUFF");
                return null;
            }

            car.engineNoises = file.ReadInts();
            car.stealWorthy = (file.ReadLine().ToLower() == "stealworthy");

            // This next section is all about impacts, 6 blocks in the order of top, bottom, left, right, front, back
            car.impactTop = new ImpactSpec(file);
            car.impactBottom = new ImpactSpec(file);
            car.impactLeft = new ImpactSpec(file);
            car.impactRight = new ImpactSpec(file);
            car.impactFront = new ImpactSpec(file);
            car.impactBack = new ImpactSpec(file);

            car.gridImages = file.ReadStrings();

            for (int i = 0; i < 3; i++)
            {
                int pixCount = file.ReadInt();

                for (int j = 0; j < pixCount; j++)
                {
                    car.pixelmaps[i].Add(file.ReadLine());
                }
            }

            int shadeCount = file.ReadInt();
            for (int j = 0; j < shadeCount; j++)
            {
                Console.WriteLine();
            }

            for (int i = 0; i < 3; i++)
            {
                int matCount = file.ReadInt();

                for (int j = 0; j < matCount; j++)
                {
                    car.materials[i].Add(file.ReadLine());
                }
            }

            int modelCount = file.ReadInt();
            for (int j = 0; j < modelCount; j++)
            {
                car.models.Add(file.ReadLine());
            }

            int actorCount = file.ReadInt();
            for (int j = 0; j < modelCount; j++)
            {
                string[] s = file.ReadStrings();
                car.actorLOD.Add(s[0].ToInt());
                car.actors.Add(s[1]);
            }

            car.reflectiveScreenMaterial = file.ReadLine();

            int steerableWheelsCount = file.ReadInt();
            for (int j = 0; j < steerableWheelsCount; j++)
            {
                car.steerableWheels.Add(file.ReadInt());
            }

            car.leftFrontSuspension = file.ReadInts();
            car.rightFrontSuspension = file.ReadInts();
            car.leftRearSuspension = file.ReadInts();
            car.rightRearSuspension = file.ReadInts();
            car.drivenWheels = file.ReadInts();
            car.nonDrivenWheels = file.ReadInts();

            car.drivenWheelDiameter = file.ReadSingle();
            car.nonDrivenWheelDiameter = file.ReadSingle();

            if (file.ReadLine() != "START OF FUNK")
            {
                //Logger.LogToFile(Logger.LogLevel.Error, "Expected \"{0}\", didn't get it.  Are you sure this is a Car.TXT file?", "START OF FUNK");
                return null;
            }

            while (file.PeekLine() != "END OF FUNK")
            {
                car.funks.Add(new Funk(file));
                if (file.PeekLine() == "NEXT FUNK") { file.ReadLine(); }
            }
            file.ReadLine();

            if (file.ReadLine() != "START OF GROOVE")
            {
                //Logger.LogToFile(Logger.LogLevel.Error, "Expected \"{0}\", didn't get it.  Are you sure this is a Car.TXT file?", "START OF GROOVE");
                return null;
            }

            while (file.PeekLine() != "END OF GROOVE")
            {
                car.grooves.Add(new Groove(file));
                if (file.PeekLine() == "NEXT GROOVE") { file.ReadLine(); }
            }
            file.ReadLine();

            for (int i = 0; i < car.actors.Count; i++)
            {
                car.crushes.Add(new br_crush(file));
                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", car.actors[i], car.crushes[i].SoftnessFactor, car.crushes[i].FoldFactor.X, car.crushes[i].WibbleFactor, car.crushes[i].LimitDeviant, car.crushes[i].SplitChance, car.crushes[i].MinYFoldDown);
            }

            int mechanicsVersion = file.ReadLine().Replace("START OF MECHANICS STUFF version ", "", StringComparison.InvariantCultureIgnoreCase).ToInt();

            car.lrWheelPos = file.ReadVector3();
            car.rrWheelPos = file.ReadVector3();
            car.lfWheelPos = file.ReadVector3();
            car.rfWheelPos = file.ReadVector3();
            car.centreOfMass = file.ReadVector3();

            switch (mechanicsVersion)
            {
                case 2:
                    int boundingBoxCount = file.ReadInt();
                    for (int i = 0; i < boundingBoxCount; i++)
                    {
                        car.boundingBoxes.Add(new br_bounds { Min = file.ReadVector3(), Max = file.ReadVector3() });
                    }
                    break;

                case 3:
                case 4:
                    car.boundingBoxes.Add(new br_bounds { Min = file.ReadVector3(), Max = file.ReadVector3() });

                    int additionalPointsCount = file.ReadInt();
                    for (int i = 0; i < additionalPointsCount; i++)
                    {
                        car.additionalPoints.Add(file.ReadVector3());
                    }
                    break;
            }

            car.turningCircleRadius = file.ReadSingle();
            car.suspensionGive = file.ReadVector2();
            car.rideHeight = file.ReadSingle();
            car.dampingFactor = file.ReadSingle();
            car.massInTonnes = file.ReadSingle();
            car.fReduction = file.ReadSingle();
            car.frictionAngle = file.ReadVector2();
            car.widthHeightLength = file.ReadVector3();
            car.tractionFractionalMultiplier = file.ReadSingle();
            car.downforceSpeed = file.ReadSingle();
            car.brakeMultiplier = file.ReadSingle();
            car.increaseInBrakesPerSecond = file.ReadSingle();
            car.rollingResistance = file.ReadVector2();
            car.numberOfGears = file.ReadInt();
            car.redLineSpeed = file.ReadInt();
            car.accelerationInHighestGear = file.ReadSingle();

            if (file.ReadLine() != "END OF MECHANICS STUFF")
            {
                //Logger.LogToFile(Logger.LogLevel.Error, "Expected \"{0}\", didn't get it.  Are you sure this is a Car.TXT file?", "END OF MECHANICS STUFF");
                return null;
            }

            int shrapnelCount = file.ReadInt();
            for (int i = 0; i < shrapnelCount; i++)
            {
                car.shrapnel.Add(file.ReadLine());
            }

            if (!file.EOF)
            {
                for (int i = 0; i < 12; i++)
                {
                    car.firePoints.Add(file.ReadInt());
                }
            }

            if (!file.EOF)
            {
                Console.WriteLine("Still data to parse in {0}", path);
            }

            return car;
        }
    }

    public class ImpactSpec
    {
        List<ImpactSpecClause> clauses;

        public ImpactSpec()
        {
            clauses = new List<ImpactSpecClause>();
        }

        public ImpactSpec(DocumentParser file)
            : this()
        {
            int clauseCount = file.ReadInt();

            for (int i = 0; i < clauseCount; i++)
            {
                clauses.Add(new ImpactSpecClause(file));
            }
        }
    }

    public class ImpactSpecClause
    {
        string clause;
        List<ImpactSpecClauseSystem> systems;

        public ImpactSpecClause()
        {
            systems = new List<ImpactSpecClauseSystem>();
        }

        public ImpactSpecClause(DocumentParser file)
            : this()
        {
            clause = file.ReadLine();

            int systemCount = file.ReadInt();

            for (int i = 0; i < systemCount; i++)
            {
                systems.Add(new ImpactSpecClauseSystem(file.ReadStrings()));
            }
        }
    }

    public class ImpactSpecClauseSystem
    {
        ImpactSpecClauseSystemPart part;
        float damage;

        public ImpactSpecClauseSystem(string[] parts)
        {
            part = parts[0].ToEnum<ImpactSpecClauseSystemPart>();
            damage = parts[1].ToSingle();
        }
    }

    public class Funk
    {
        string material;
        FunkMode mode;
        FunkMatrixMode matrixModType;
        GroovePathMode matrixModMode;
        float spinPeriod;
        br_vector2 rollPeriods;
        GroovePathMode lightingMode;
        FunkAnimationType animationType;

        public Funk() { }

        public Funk(DocumentParser file)
            : this()
        {
            material = file.ReadLine();
            mode = file.ReadLine().ToEnum<FunkMode>();
            matrixModType = file.ReadLine().ToEnum<FunkMatrixMode>();
            if (matrixModType != FunkMatrixMode.None) { matrixModMode = file.ReadLine().ToEnum<GroovePathMode>(); }

            switch (matrixModType)
            {
                case FunkMatrixMode.roll:
                    rollPeriods = file.ReadVector2();
                    break;

                case FunkMatrixMode.spin:
                    spinPeriod = file.ReadSingle();
                    break;

                default:
                    Console.WriteLine(file.ToString());
                    break;
            }

            lightingMode = file.ReadLine().ToEnumWithDefault<GroovePathMode>(GroovePathMode.None);
            if (lightingMode != GroovePathMode.None)
            {
                Console.WriteLine(file.ToString());
            }

            animationType = file.ReadLine().ToEnumWithDefault<FunkAnimationType>(FunkAnimationType.None);
            if (animationType != FunkAnimationType.None)
            {
                Console.WriteLine(file.ToString());
            }
        }
    }

    public class Groove
    {
        string part;
        LollipopMode lollipopMode;
        GrooveMode mode;
        GroovePathNames pathType;
        GroovePathMode pathMode;
        br_vector3 pathCentre;
        float pathPeriod;
        br_vector3 pathDelta;
        GrooveAnimation animationType;
        GroovePathMode animationMode;
        float animationPeriod;
        br_vector3 animationCentre;
        GrooveAnimationAxis animationAxis;
        float rockMaxAngle;
        br_vector3 shearPeriod;
        br_vector3 shearMagnitude;

        public Groove() { }

        public Groove(DocumentParser file)
            : this()
        {
            part = file.ReadLine();
            lollipopMode = file.ReadLine().ToEnumWithDefault<LollipopMode>(LollipopMode.NotALollipop);
            mode = file.ReadLine().ToEnum<GrooveMode>();
            pathType = file.ReadLine().ToEnumWithDefault<GroovePathNames>(GroovePathNames.None);
            if (pathType != GroovePathNames.None) { pathMode = file.ReadLine().ToEnum<GroovePathMode>(); }

            switch (pathType)
            {
                case GroovePathNames.None:
                    break;

                case GroovePathNames.straight:
                    pathCentre = file.ReadVector3();
                    pathPeriod = file.ReadSingle();
                    pathDelta = file.ReadVector3();
                    break;

                default:
                    Console.WriteLine();
                    break;
            }

            animationType = file.ReadLine().ToEnumWithDefault<GrooveAnimation>(GrooveAnimation.None);
            if (animationType != GrooveAnimation.None) { animationMode = file.ReadLine().ToEnum<GroovePathMode>(); }

            switch (animationType)
            {
                case GrooveAnimation.rock:
                    animationPeriod = file.ReadSingle();
                    animationCentre = file.ReadVector3();
                    animationAxis = file.ReadLine().ToEnum<GrooveAnimationAxis>();
                    rockMaxAngle = file.ReadSingle();
                    break;

                case GrooveAnimation.shear:
                    shearPeriod = file.ReadVector3();
                    animationCentre = file.ReadVector3();
                    shearMagnitude = file.ReadVector3();
                    break;

                case GrooveAnimation.spin:
                    animationPeriod = file.ReadSingle();
                    animationCentre = file.ReadVector3();
                    animationAxis = file.ReadLine().ToEnum<GrooveAnimationAxis>();
                    break;

                default:
                    Console.WriteLine(file.ToString());
                    break;
            }
        }
    }
}
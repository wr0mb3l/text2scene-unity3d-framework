using System.Collections.Generic;
using UnityEngine;


    /// <summary>
    /// New IsoEntity, which contains the three new fields Holonym, DisambiguationWord and Praefix
    /// </summary>
    public class ClassifiedObject : IsoSpatialEntity
    {
        public string Holonym { get; set; }
        public string DisambiguationWord { get; set; }
        /// <summary>
        /// Bert prefix
        /// </summary>
        public string Praefix { get; set; }

        /// <summary>
        /// Section for semisupervised Learning, holds all objects for training
        /// </summary>
        public List<string> InputTestWords { get; set; }

        public ClassifiedObject(List<string> inputTestWords, string word, string partNetWord, string disambiguationWord) :
            base(new AnnotationDocument(0, "This is a really long text to get some space"), 1, 0, 15, null, null, null, null, null, null, word, null, null, DimensionType.none, FormType.none,
                false, null, null, null, null, false, null, null)
        {
            InputTestWords = inputTestWords;
            Holonym = partNetWord;
            DisambiguationWord = disambiguationWord;
        }
        /// </summary>

        public ClassifiedObject(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position,
            IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string type,
            string word, string partNetWord, string disambiguationWord, string praefix, string mod, string spatial_entity, DimensionType dim, FormType form, bool dcl,
            string domain, string lat, string lon, IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes) :
            base(parent, ID, begin, end, object_ID, position,
                rotation, scale, object_feature, type,
                word, mod, spatial_entity, dim, form, dcl,
                domain, lat, lon, elevation, countable, gquant, scopes)
        {
            Holonym = partNetWord;
            DisambiguationWord = disambiguationWord;
            Praefix = praefix;
        }

        /// <summary>
        /// Simplified Classified Object constructor
        /// </summary>
        public ClassifiedObject(AnnotationBase parent, int ID, int begin, int end, string word, string partNetWord, string disambiguationWord, string praefix) :
            base(parent, ID, begin, end, null, null, null, null, null, word, null, null, DimensionType.volume, FormType.none, false,
                null, null, null, null, false, null, null, false)
        {
            Holonym = partNetWord;
            DisambiguationWord = disambiguationWord;
            Praefix = praefix;
        }

        public ClassifiedObject(AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position,
                            IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string word, string partNetWord,
                            string disambiguationWord, string praefix, string mod, string spatial_entity, DimensionType dim, FormType form,
                            bool dcl, string domain, string lat, string lon, IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes) :
            base(parent, ID, begin, end, object_ID, position,
                rotation, scale, object_feature,
                word, mod, spatial_entity, dim, form, dcl,
                domain, lat, lon, elevation, countable, gquant, scopes)
        {
            Holonym = partNetWord;
            DisambiguationWord = disambiguationWord;
            Praefix = praefix;
        }


        /// <summary>
        /// Constructor für semisupervised learning
        /// </summary>
        public ClassifiedObject(List<string> inputTestWords, AnnotationBase parent, int ID, int begin, int end, string object_ID, IsoVector3 position,
            IsoVector4 rotation, IsoVector3 scale, List<IsoObjectAttribute> object_feature, string type,
            string word, string partNetWord, string disambiguationWord, string mod, string spatial_entity, DimensionType dim, FormType form, bool dcl,
            string domain, string lat, string lon, IsoMeasure elevation, bool countable, string gquant, List<IsoEntity> scopes) :
            base(parent, ID, begin, end, object_ID, position,
                rotation, scale, object_feature, type,
                word, mod, spatial_entity, dim, form, dcl,
                domain, lat, lon, elevation, countable, gquant, scopes)
        {
            Holonym = partNetWord;
            DisambiguationWord = disambiguationWord;
            InputTestWords = inputTestWords;
        }

        public void SetShapeNetObject(GameObject gameObject, string object_ID)
        {
            SetObjectID(object_ID);
            SetShapeNetObject(gameObject);
        }

        /// <summary>
        /// Creates the vector for scale, position and rotation
        /// </summary>
        /// <param name="gameObject"></param>
        public void SetShapeNetObject(GameObject gameObject)
        {
            System.Random rand = new System.Random();
            IsoVector3 iScale = new IsoVector3(rand.Next(10000000, 33333333), gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z, (AnnotationDocument)Parent);
            SetScale(iScale);
            IsoVector3 iPos = new IsoVector3(rand.Next(33333333, 66666666), gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z, (AnnotationDocument)Parent);
            SetPosition(iPos);
            IsoVector4 iRot = new IsoVector4(rand.Next(66666667, 99999999), gameObject.transform.rotation.x, gameObject.transform.rotation.y, gameObject.transform.rotation.z, gameObject.transform.rotation.w, (AnnotationDocument)Parent);
            SetRotation(iRot);
        }

        /// <summary>
        /// Updates scale, position and rotation for a shapenet object
        /// </summary>
        /// <param name="gameObject"></param>
        public void UpdateShapeNetObject(GameObject gameObject)
        {
            if (Scale != null)
            {
                IsoVector3 scale = ((AnnotationDocument)Parent).GetElementByID<IsoVector3>((int)Scale.ID, false);
                scale.SetVector(gameObject.transform.localScale);
            }

            if (Position != null)
            {
                IsoVector3 pos = ((AnnotationDocument)Parent).GetElementByID<IsoVector3>((int)Position.ID, false);
                pos.SetVector(gameObject.transform.position);
            }

            if (Rotation != null)
            {
                IsoVector4 rot = ((AnnotationDocument)Parent).GetElementByID<IsoVector4>((int)Rotation.ID, false);
                rot.SetQuaternion(gameObject.transform.rotation);
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ClassifiedObject);
        }

        public bool Equals(ClassifiedObject other)
        {
            return other != null &&
                   Holonym == other.Holonym &&
                   DisambiguationWord == other.DisambiguationWord &&
                   ID == other.ID &&
                   Begin == other.Begin &&
                   End == other.End &&
                   Comment == other.Comment;
        }

        public override int GetHashCode()
        {
            int hashCode = 1689935802;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Holonym);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisambiguationWord);
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            hashCode = hashCode * -1521134295 + Begin.GetHashCode();
            hashCode = hashCode * -1521134295 + End.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Comment);
            return hashCode;
        }

        public static bool operator ==(ClassifiedObject left, ClassifiedObject right)
        {
            return EqualityComparer<ClassifiedObject>.Default.Equals(left, right);
        }

        public static bool operator !=(ClassifiedObject left, ClassifiedObject right)
        {
            return !(left == right);
        }
    }

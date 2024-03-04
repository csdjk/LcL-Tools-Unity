using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
#if FBX_EXPORTER
using UnityEditor.Formats.Fbx.Exporter;
#endif

using UnityEngine;
namespace LcLTools
{

    [InitializeOnLoad]
    public class CheckFBXSupport
    {
        static CheckFBXSupport()
        {
            // AssetDatabase.AllowAutoRefresh();
            CheckAndAddFBXSupport();
            // AssetDatabase.DisallowAutoRefresh();
            LcL_RenderingPipelineDefines.AddDefine("FBX_EXPORTER");
        }


        private static void CheckAndAddFBXSupport()
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            string manifestText = File.ReadAllText(manifestPath);

            if (!manifestText.Contains("com.unity.formats.fbx"))
            {
                manifestText = manifestText.Replace("\"dependencies\": {", "\"dependencies\": {\n    \"com.unity.formats.fbx\": \"4.1.3\",");
                File.WriteAllText(manifestPath, manifestText);
            }
        }
    }
#if FBX_EXPORTER

    public class JaveLin_RDC_CSV2FBX : EditorWindow
    {
        [MenuItem("LcLTools/CSV To FBX")]
        private static void _Show()
        {
            var win = EditorWindow.GetWindow<JaveLin_RDC_CSV2FBX>();
            win.titleContent = new GUIContent("JaveLin_RDC_CSV2FBX");
            win.Show();
        }

        public class VertexIDInfo
        {
            public int IDX;
            public VertexInfo vertexInfo;
        }

        public enum SemanticType
        {
            Unknown,

            VTX,

            IDX,

            POSITION_X,
            POSITION_Y,
            POSITION_Z,
            POSITION_W,

            NORMAL_X,
            NORMAL_Y,
            NORMAL_Z,
            NORMAL_W,

            TANGENT_X,
            TANGENT_Y,
            TANGENT_Z,
            TANGENT_W,

            TEXCOORD0_X,
            TEXCOORD0_Y,
            TEXCOORD0_Z,
            TEXCOORD0_W,

            TEXCOORD1_X,
            TEXCOORD1_Y,
            TEXCOORD1_Z,
            TEXCOORD1_W,

            TEXCOORD2_X,
            TEXCOORD2_Y,
            TEXCOORD2_Z,
            TEXCOORD2_W,

            TEXCOORD3_X,
            TEXCOORD3_Y,
            TEXCOORD3_Z,
            TEXCOORD3_W,

            TEXCOORD4_X,
            TEXCOORD4_Y,
            TEXCOORD4_Z,
            TEXCOORD4_W,

            TEXCOORD5_X,
            TEXCOORD5_Y,
            TEXCOORD5_Z,
            TEXCOORD5_W,

            TEXCOORD6_X,
            TEXCOORD6_Y,
            TEXCOORD6_Z,
            TEXCOORD6_W,

            TEXCOORD7_X,
            TEXCOORD7_Y,
            TEXCOORD7_Z,
            TEXCOORD7_W,

            COLOR0_X,
            COLOR0_Y,
            COLOR0_Z,
            COLOR0_W,
        }

        // jave.lin : Semantic 闂傚倸鍊风粈渚€骞栭銈傚亾閿燂拷?濠电姷鏁搁崑鎰板磻閹剧粯鈷戦弶鐐村閸斿秹鏌熼悷鐗堟悙??濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕⒒閸屾艾鈧嘲霉閸パ€鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
        public enum SemanticMappingType
        {
            Default,            // jave.lin : 濠电姷鏁搁崑鐘诲箵椤忓棗绶ら柦姗堟嫹?缁绢參顥撶弧鈧悗纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦纰嶇涵鐐亜椤愶綆娈旈柍钘夘槸閳诲骸鈻庨幒鍡椾壕闁秆勵殕閻撴稑霉閿濆牜娼愰悘蹇曞缁绘盯鎮℃惔锝囶啋閻庤娲橀敃锟�?闊洦鎸炬牎濠电偛鐗呴崡鍐差潖濞差亝鍋￠柡澶嬪椤斺偓闂備胶鎳撻崵鏍箯閿燂拷
            ManuallyMapping,    // jave.lin : 濠电姷鏁搁崑鐘诲箵椤忓棗绶ら柦姗堟嫹?缁绢參顥撶弧鈧悗纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂鍙ョ敖濠碉紕鍋戦崐鎴﹀垂閾忕懓鍨濋幖娣妼缁狀垶鎮楅敓锟�?闂佺懓寮堕幐鍐茬暦閻旂⒈鏁�?婵炴潙鍚嬮幑鍥ь潖缂佹ɑ濯撮柤鎭掑劤閵嗗﹪姊洪崫銉バｉ柛鏃€鐟ラ锝夊川婵犲嫮鐦堝┑顕嗘嫹?缂備緤鎷�?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃锟�??婵炲鍘ч悺銊╁磻閹邦喚纾藉ù锝堝亗閹存績鏋嶉柕蹇嬪€栭埛鎴︽煕濠靛棗顏╅柍褜鍓欓悥鐓庣暦閹版澘鍐€?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍鐐烘⒑缁嬫寧婀扮紒瀣灴椤拷?
        }

        // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備胶鎳撴晶鐣屽垝椤栫偛纾挎俊銈勯檷娴滄粓鏌熼幍铏珔闁逞屽墯閻楃娀骞冩ィ鍐╃劶鐎广儱妫涢崢閬嶆⒑閻熸壆鎽犻柣鐔村劦閸┾偓?闂佺尨鎷�?闂佺鎷�?濠殿喗枪妞存悂宕甸埀顒€顪冮妶鍡樼┛缂傚秳绶氬顐﹀箻缂佹ɑ娅㈤梺鍖℃嫹?
        public enum MaterialSetType
        {
            CreateNew,
            UsingExsitMaterialAsset,
        }

        // jave.lin : application to vertex shader 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閼归偊鏆伴梻浣呵归鍡涘箰閹间緤缍栨繝闈涱儐閸ゅ鏌涢敓锟�?濠电偛鎳岄崹钘夘潖濞差亜浼犻柛鏇ㄥ亐閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備胶鎳撴晶鐣屽垝椤栫偞鍋傞柍褜鍓涚槐鎺楁倷椤掍胶鍑″銈忕畳娴滎剛鍒掗弮鍫濋唶闁哄洨鍠撻崢鍨繆閻愬樊鍎忓Δ鐘虫倐瀹曘垽骞橀鐣屽幐婵炶揪缍€椤鐣峰畝鈧埀顒婃嫹?濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕⒒閸屾艾鈧嘲霉閸パ€鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鐒﹂惇褰掑磹閵堝鍨傞弶鍫氭櫇閻牊銇勯弴妤€浜惧┑顔硷龚濞咃綁骞戦崟顖毼╅柕澶涘閳ь剟绠栧娲传閿燂拷??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忛崫鍔借櫣鎷犻幓鎺戭劀闂備胶枪椤戝棝骞愰崜褎顫曢柣鎰惈閻愬﹥銇勯幒鍡椾壕缂備緤鎷�?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�?
        public class VertexInfo
        {
            public int VTX;
            public int IDX;

            public float POSITION_X;
            public float POSITION_Y;
            public float POSITION_Z;
            public float POSITION_W;

            public float NORMAL_X;
            public float NORMAL_Y;
            public float NORMAL_Z;
            public float NORMAL_W;

            public float TANGENT_X;
            public float TANGENT_Y;
            public float TANGENT_Z;
            public float TANGENT_W;

            public float TEXCOORD0_X;
            public float TEXCOORD0_Y;
            public float TEXCOORD0_Z;
            public float TEXCOORD0_W;

            public float TEXCOORD1_X;
            public float TEXCOORD1_Y;
            public float TEXCOORD1_Z;
            public float TEXCOORD1_W;

            public float TEXCOORD2_X;
            public float TEXCOORD2_Y;
            public float TEXCOORD2_Z;
            public float TEXCOORD2_W;

            public float TEXCOORD3_X;
            public float TEXCOORD3_Y;
            public float TEXCOORD3_Z;
            public float TEXCOORD3_W;

            public float TEXCOORD4_X;
            public float TEXCOORD4_Y;
            public float TEXCOORD4_Z;
            public float TEXCOORD4_W;

            public float TEXCOORD5_X;
            public float TEXCOORD5_Y;
            public float TEXCOORD5_Z;
            public float TEXCOORD5_W;

            public float TEXCOORD6_X;
            public float TEXCOORD6_Y;
            public float TEXCOORD6_Z;
            public float TEXCOORD6_W;

            public float TEXCOORD7_X;
            public float TEXCOORD7_Y;
            public float TEXCOORD7_Z;
            public float TEXCOORD7_W;

            public float COLOR0_X;
            public float COLOR0_Y;
            public float COLOR0_Z;
            public float COLOR0_W;

            public Vector3 POSITION
            {
                get
                {
                    return new Vector3(
                    POSITION_X,
                    POSITION_Y,
                    POSITION_Z);
                }
            }

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備胶鎳撴晶鐣屽垝椤栫偛鐤鹃柟闂寸劍閻撱儵鏌ｉ弴鐐测偓鍦偓姘炬嫹
            public Vector4 POSITION_H
            {
                get
                {
                    return new Vector4(
                    POSITION_X,
                    POSITION_Y,
                    POSITION_Z,
                    1);
                }
            }

            public Vector4 NORMAL
            {
                get
                {
                    return new Vector4(
                    NORMAL_X,
                    NORMAL_Y,
                    NORMAL_Z,
                    NORMAL_W);
                }
            }
            public Vector4 TANGENT
            {
                get
                {
                    return new Vector4(
                    TANGENT_X,
                    TANGENT_Y,
                    TANGENT_Z,
                    TANGENT_W);
                }
            }

            public Vector4 TEXCOORD0
            {
                get
                {
                    return new Vector4(
                    TEXCOORD0_X,
                    TEXCOORD0_Y,
                    TEXCOORD0_Z,
                    TEXCOORD0_W);
                }
            }

            public Vector4 TEXCOORD1
            {
                get
                {
                    return new Vector4(
                    TEXCOORD1_X,
                    TEXCOORD1_Y,
                    TEXCOORD1_Z,
                    TEXCOORD1_W);
                }
            }

            public Vector4 TEXCOORD2
            {
                get
                {
                    return new Vector4(
                    TEXCOORD2_X,
                    TEXCOORD2_Y,
                    TEXCOORD2_Z,
                    TEXCOORD2_W);
                }
            }

            public Vector4 TEXCOORD3
            {
                get
                {
                    return new Vector4(
                    TEXCOORD3_X,
                    TEXCOORD3_Y,
                    TEXCOORD3_Z,
                    TEXCOORD3_W);
                }
            }

            public Vector4 TEXCOORD4
            {
                get
                {
                    return new Vector4(
                    TEXCOORD4_X,
                    TEXCOORD4_Y,
                    TEXCOORD4_Z,
                    TEXCOORD4_W);
                }
            }

            public Vector4 TEXCOORD5
            {
                get
                {
                    return new Vector4(
                    TEXCOORD5_X,
                    TEXCOORD5_Y,
                    TEXCOORD5_Z,
                    TEXCOORD5_W);
                }
            }

            public Vector4 TEXCOORD6
            {
                get
                {
                    return new Vector4(
                    TEXCOORD6_X,
                    TEXCOORD6_Y,
                    TEXCOORD6_Z,
                    TEXCOORD6_W);
                }
            }

            public Vector4 TEXCOORD7
            {
                get
                {
                    return new Vector4(
                    TEXCOORD7_X,
                    TEXCOORD7_Y,
                    TEXCOORD7_Z,
                    TEXCOORD7_W);
                }
            }

            public Color COLOR0
            {
                get
                {
                    return new Color(
                    COLOR0_X,
                    COLOR0_Y,
                    COLOR0_Z,
                    COLOR0_W);
                }
            }
        }

        private const string GO_Parent_Name = "Models_From_CSV";

        // jave.lin : on_gui 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?濠电姭鍋撻弶鍫氭櫅閸ㄦ繈鏌涢锝囪穿婵炶鎷�?闊洦鎸婚崳鐑樼箾閸涱偄鐏紒缁樼洴楠炴﹢寮堕幋鐐村劎闂備胶枪鐎涒晠骞愰幎濮愨偓浣糕枎閹惧磭鐤€闂佸搫顦冲▔锟�??闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
        private TextAsset RDC_Text_Asset;
        private string fbxName;
        private string outputDir;
        private string outputFullName;

        // jave.lin : on_gui - options
        private Vector2 optionsScrollPos;
        private static bool options_show = true;
        private static bool is_from_DX_CSV = true;
        private static Vector3 vertexOffset = Vector3.zero;
        private static Vector3 vertexRotation = Vector3.zero;
        private static Vector3 vertexScale = Vector3.one;
        private static bool is_reverse_vertex_order = false; // jave.lin : for reverse normal
        private static bool is_recalculate_bound = true;
        private static SemanticMappingType semanticMappingType = SemanticMappingType.Default;
        private static bool has_uv0 = true;
        private static bool has_uv1 = false;
        private static bool has_uv2 = false;
        private static bool has_uv3 = false;
        private static bool has_uv4 = false;
        private static bool has_uv5 = false;
        private static bool has_uv6 = false;
        private static bool has_uv7 = false;
        private static bool has_color0 = false;
        private static bool useAutoMapping = false;
        private static bool useAllComponent = true;
        private ModelImporterNormals normalImportType = ModelImporterNormals.Import;
        private ModelImporterTangents tangentImportType = ModelImporterTangents.Import;
        private bool show_mat_toggle = true;
        private MaterialSetType materialSetType = MaterialSetType.CreateNew;
        private Shader shader;
        private Texture texture;
        private Material material;

        // jave.lin : helper 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
        private Dictionary<string, SemanticType> semanticTypeDict_key_name_helper;
        private Dictionary<string, SemanticType> semanticManullyMappingTypeDict_key_name_helper;
        private static Dictionary<string, SemanticType> semanticManullyMappingTypeDict_Cache = new Dictionary<string, SemanticType>();


        private SemanticType[] semanticsIDX_helper;
        private int[] semantics_check_duplicated_helper;
        private List<string> stringListHelper;

        private int[] GetSemantics_check_duplicated_helper()
        {
            if (semantics_check_duplicated_helper == null)
            {
                var vals = Enum.GetValues(typeof(SemanticType));
                semantics_check_duplicated_helper = new int[vals.Length];
                for (int i = 0; i < vals.Length; i++)
                {
                    semantics_check_duplicated_helper[i] = 0;
                }
            }
            return semantics_check_duplicated_helper;
        }

        private void ClearSemantics_check_duplicated_helper(int[] arr)
        {
            if (arr != null)
            {
                Array.Clear(arr, 0, arr.Length);
            }
        }

        private List<string> GetStringListHelper()
        {
            if (stringListHelper == null)
            {
                stringListHelper = new List<string>();
            }
            return stringListHelper;
        }

        // jave.lin : 闂傚倸鍊风粈渚€骞夐敍鍕殰闁绘劕顕粻楣冩煃閿燂拷?闂傚倷娴囧銊╂倿閿曞倸绠查柛銉嫹??濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕⒒閸屾瑧顦︽繝鈧潏銊т粴婵犵數鍋涘Ο濠囧矗閸愵煈鍤�?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝夋煛瀹€瀣М闁糕斁鍓濈换婵嬪磼濞戞矮閭紓鍌氬€搁崐鍝ョ矙閸曨垰绠�?+闂傚倸鍊烽懗鍫曞磿閻㈢ǹ鐤鹃柍鍝勬噹缁愭鏌￠敓锟�?缂備浇浜崑锟�?闁靛鎷�?閻庯綆鍓欏鎶芥⒒娴ｈ櫣甯涢柡灞诲姂瀹曘儳鈧綆鍠栫粻鐘差熆鐠鸿櫣鐏辨俊顐灦閺屸剝寰勯敓锟�?闁绘劦鍎疯閺岋絾鎯旈姀锝咁棟缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾濞差亝鈷戦柤娴嬫櫅鐢埖绻涢弶鎴濇Щ闁宠棄顦甸幃浠嬪川婵犲嫬寮抽梺璇插嚱缂嶅棙绂嶉懜闈涱嚤?
        private void DelectDir(string dir)
        {
            try
            {
                if (!Directory.Exists(outputDir))
                    return;

                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                // 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏堢嵁閸℃稑绀�?濡炪們鍨洪悷鈺呭箠閺嶎厼鐓涢柛鎰舵嫹?闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夐崕锟�?闁告縿鍎查崵宀勬⒒娓氣偓濞佳囨晬韫囨梹濯撮柟缁樺笂婢规洟鏌ｆ惔顖滅У闁哥姵鐗滅划锟�?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼闂侀潻鎷�?闂佺ǹ绻戦敃銏ｆ＂闂佽宕樼粔顔济洪鍕唵闁诲繒鍋犻濠傂уΔ鍛拺閻犳亽鍔屽▍鎰版煙閿燂拷?
                FileSystemInfo[] fileInfos = dirInfo.GetFileSystemInfos();
                foreach (FileSystemInfo fileInfo in fileInfos)
                {
                    if (fileInfo is DirectoryInfo)
                    {
                        // 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭骞栧ǎ顒€鈧垶绂嶈ぐ鎺撶厪濠电倯鍐仼?濠靛倸鎲￠崑鈩冪箾閸℃绠�??婵＄偑鍊曠换鎰版偉閻撳寒娼栭柧蹇氼潐瀹曞鏌曟繛鍨姕闁诲繐妫楅—鍐Χ鎼粹€茬凹濡炪們鍔岄敃銈夋偩瀹勬壋鏋庨柟鐐窞瑜旈弻娑㈠焺閿燂拷?濠电姳鑳舵径鍕⒒閸屾艾鈧嘲霉閸パ€鏋栭柡鍥ュ灩缁愭鈧箍鍎遍ˇ浼村吹閹达箑绠归弶鍫濆⒔缁嬪鏌涜箛鏃傜煉婵﹥妞藉畷锟�?濡炪倖鍨甸幊锟�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟﹀▍銏ゆ⒑鐠恒劌娅愰柟鍑ゆ嫹
                        DirectoryInfo subDir = new DirectoryInfo(fileInfo.FullName);
                        subDir.Delete(true);            // 闂傚倸鍊风粈渚€骞夐敍鍕殰闁绘劕顕粻楣冩煃閿燂拷?闂傚倷娴囧銊╂倿閿曞倸绠查柛銉嫹??濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕⒒閸屾艾鈧嘲霉閸パ€鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?婵犵绱曢崑锟�??闁诲孩绋堥弲婵堝垝閿濆憘鏃堝礃閳轰焦鐎鹃梻浣虹帛椤ㄥ懘鎮ф繝鍕焼閻庯綆鍠楅悡娑樏归敐鍫綈鐎规洖鐬奸埀顒婃嫹?濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕⒒閸屾艾鈧嘲霉閸パ€鏋栭柡鍥ュ灩缁愭鈧箍鍎遍ˇ浼村吹閹达箑绠归弶鍫濆⒔缁嬪鏌涜箛鏃傜煉婵﹥妞藉畷锟�?濡炪倖鍨甸幊锟�?闊洢鍎茬€氾拷
                    }
                    else
                    {
                        File.Delete(fileInfo.FullName);      // 闂傚倸鍊风粈渚€骞夐敍鍕殰闁绘劕顕粻楣冩煃閿燂拷?闂傚倷娴囧銊╂倿閿曞倸绠查柛銉嫹??濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕⒒閸屾瑧顦︽繝鈧潏銊т粴婵犵數鍋涘Ο濠囧矗閸愵煈鍤�?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾濞差亝鈷戦柤娴嬫櫅鐢埖绻涢弶鎴濇Щ闁宠棄顦甸幃浠嬪川婵犲嫬寮抽梺璇插嚱缂嶅棙绂嶉懜闈涱嚤?
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃銈夋偩瀹勬嫈鏃堝川椤撳洠鏅犻弻鏇熺箾瑜嶉幊搴ㄥ极閸洘鈷掗柛灞剧懄缁佺増绻涙径瀣鐎规洑鍗抽獮鍥级閸喖娈ゅ┑鐘垫暩婵敻鎳濋崜褏涓�?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 闂傚倷绀侀幖锟�?閻忕偠妫勬竟澶愭⒑閸涘﹥灏甸柛鐘崇墪椤曪拷?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦纰嶇涵楣冩煃閽樺妯€妤犵偞鐟╁畷锟�?闁诲函鎷�? assets 闂傚倸鍊烽懗鍫曞磿閻㈢ǹ鐤鹃柍鍝勬噹缁愭鏌￠敓锟�?缂備浇浜崑锟�?闁靛鎷�?閻庯綆鍓欏鎶芥⒒娴ｈ櫣甯涢柡灞诲姂瀹曘儳鈧綆鍠栫粻鐘差熆鐠鸿櫣鐏辨俊顐灦閺屸剝寰勯敓锟�?闁绘劦鍎疯閺岋絾鎯旈姀锝咁棟缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洢鍎茬€氾拷
        private string GetAssetPathByFullName(string fullName)
        {
            fullName = fullName.Replace("\\", "/");
            var dataPath_prefix = Application.dataPath.Replace("Assets", "");
            dataPath_prefix = dataPath_prefix.Replace(dataPath_prefix + "/", "");
            var mi_path = fullName.Replace(dataPath_prefix, "");
            return mi_path;
        }

        private void OnGUI()
        {
            Output_RDC_CSV_Handle();
        }

        /// <summary>
        /// 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊冣攽閻愯泛鐨虹紒顕呭灦瀵拷?闂傚倸鍊风粈渚€骞夐敓鐘插瀭?闂備胶枪閿曘儵鎮у⿰鍫濈劦?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鐓熼柕蹇婃櫅閻忕娀鏌ｆ幊閸旓拷?闁绘挸娴风粻濠氭煙椤旂晫鎳�?闁瑰搫妫楁禍鎯ь渻閵堝懐绠伴柣鏍帶椤繘鎼圭憴鍕／闂侀潧枪閸庢煡鎮楁ィ鍐┾拺闁告繐鎷�??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄤ粙寮婚敐澶嬪€烽悗鐢电《婵洤顪冮妶鍐ㄥ闁绘妫濋崺銏ゅ箻鐠囪尙顓洪梺缁樺姇椤曨厾绮旈崼鏇熲拺缂備焦锚婵箓鏌涢幘瀵告噰?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟﹀▍銏ゆ⒑鐠恒劌娅愰柟鍑ゆ嫹
        /// </summary>
        /// <param name="str">闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌￠敓锟�?闂侀潧妫旂粈锟�?濠电姴鍊绘晶鏇犵磼閳ь剟宕熼娑氬帾闂佸壊鍋呯换锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟﹀▍銏ゆ⒑鐠恒劌娅愰柟鍑ゆ嫹</param>
        /// <param name="substring">闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼敓锟�?闂佺尨鎷�?闂佽瀛╃粙鎺曟懌缂傚倸绉村ú顓㈠蓟閿濆绠涢梻鍫熺☉椤晛顪冮妶鍛寸崪闁瑰嚖鎷�</param>
        /// <returns>闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁稿濮电换娑㈠箣閻愬灚鍣藉┑鐐茬墛钃辩紒缁樼洴瀹曞ジ鎮㈡搴濇婵犵數鍋熼崢褏鎹㈠鈧獮鍐ㄢ枎閿燂拷?闁瑰搫妫楁禍鎯ь渻閵堝懐绠伴柣鏍帶椤繘鎼圭憴鍕／闂侀潧枪閸庢煡鎮楁ィ鍐┾拺闁告繐鎷�??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟﹀▍銏ゆ⒑鐠恒劌娅愰柟鍑ゆ嫹</returns>
        static int SubstringCount(string str, string substring)
        {
            if (str.Contains(substring))
            {
                string strReplaced = str.Replace(substring, "");
                return (str.Length - strReplaced.Length) / substring.Length;
            }

            return 0;
        }

        bool IsEquals(string semantic, string target)
        {
            semantic = semantic.ToLower();
            string type, component;
            if (semantic.Contains("_x"))
            {
                type = semantic.Replace("_x", "");
                component = ".x";
            }
            else if (semantic.Contains("_y"))
            {
                type = semantic.Replace("_y", "");
                component = ".y";
            }
            else if (semantic.Contains("_z"))
            {
                type = semantic.Replace("_z", "");
                component = ".z";
            }
            else if (semantic.Contains("_w"))
            {
                type = semantic.Replace("_w", "");
                component = ".w";
            }
            else
            {
                type = semantic;
                component = semantic;
            }

            return target.Contains(type) && target.Contains(component);
        }

        SemanticType TryGetSemanticType(string str)
        {
            var lowStr = str.ToLower();
            foreach (SemanticType st in Enum.GetValues(typeof(SemanticType)))
            {
                if (IsEquals(st.ToString(), lowStr))
                {
                    return st;
                }
            }
            return SemanticType.Unknown;
        }


        private bool refresh_data = false;
        private bool csv_asset_changed = false;
        private void Output_RDC_CSV_Handle()
        {
            var new_textAsset = EditorGUILayout.ObjectField("RDC_CSV", RDC_Text_Asset, typeof(TextAsset), false) as TextAsset;

            // RDC_Text_Asset = EditorGUILayout.ObjectField("RDC_CSV", RDC_Text_Asset, typeof(TextAsset), false) as TextAsset;

            csv_asset_changed = false;
            if (RDC_Text_Asset != new_textAsset)
            {
                csv_asset_changed = true;
                RDC_Text_Asset = new_textAsset;
            }

            if (RDC_Text_Asset == null)
            {
                var srcCol = GUI.contentColor;
                GUI.contentColor = Color.red;
                EditorGUILayout.LabelField("Have no setting the RDC_CSV yet!");
                GUI.contentColor = srcCol;
                return;
            }

            if (refresh_data || csv_asset_changed)
            {
                material = null;
                semanticManullyMappingTypeDict_key_name_helper = null;
                if (refresh_data)
                {
                    semanticManullyMappingTypeDict_Cache.Clear();
                }
                ClearSemantics_check_duplicated_helper(semantics_check_duplicated_helper);
            }

            fbxName = EditorGUILayout.TextField("FBX Name", fbxName);
            if (RDC_Text_Asset != null && (refresh_data || csv_asset_changed || string.IsNullOrEmpty(fbxName)))
            {
                fbxName = GenerateGOName(RDC_Text_Asset);
            }

            // jave.lin : output path
            EditorGUILayout.BeginHorizontal();
            outputDir = EditorGUILayout.TextField("Output Path(Dir)", outputDir);
            if (refresh_data || csv_asset_changed || string.IsNullOrEmpty(outputDir))
            {
                var csvPath = LcLEditorUtilities.GetAssetAbsolutePath(RDC_Text_Asset);
                outputDir = Path.Combine(Path.GetDirectoryName(csvPath), fbxName);
                outputDir = outputDir.Replace("\\", "/");
            }
            if (GUILayout.Button("Browser...", GUILayout.Width(100)))
            {
                outputDir = EditorUtility.OpenFolderPanel("Select an output path", outputDir, "");
            }
            EditorGUILayout.EndHorizontal();
            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?婵犵绱曢崑锟�??缂備降鍔庨崕銈囩矉閹烘垟妲堥柕蹇婃閹锋椽鏌ｉ悩鍏呰埅闁告柨鐭傚鎼佸箣閿旂晫鍘搁柣搴嫹?濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼闂佽崵鍠愬妯衡枔閳哄懏鐓欓柛鎴欏€栫€氾拷 full name
            GUI.enabled = false;
            outputFullName = Path.Combine(outputDir, fbxName + ".fbx");
            outputFullName = outputFullName.Replace("\\", "/");
            EditorGUILayout.TextField("Output Full Name", outputFullName);
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                refresh_data = false;
                if (GUILayout.Button("Reset Settings"))
                {
                    refresh_data = true;
                }
                if (GUILayout.Button("Export FBX"))
                {
                    ExportHandle();
                }
            }
            GUILayout.EndHorizontal();

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?婵犵绱曢崑锟�??缂備降鍔庨崕銈囩矉閹烘鏅�? scroll view
            optionsScrollPos = EditorGUILayout.BeginScrollView(optionsScrollPos);

            // jave.lin : options 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
            EditorGUILayout.Space(10);
            options_show = EditorGUILayout.BeginFoldoutHeaderGroup(options_show, "Model Options");
            if (options_show)
            {
                EditorGUI.indentLevel++;
                is_from_DX_CSV = EditorGUILayout.Toggle("Is From DirectX CSV", is_from_DX_CSV);
                is_reverse_vertex_order = EditorGUILayout.Toggle("Is Reverse Normal", is_reverse_vertex_order);
                is_recalculate_bound = EditorGUILayout.Toggle("Is Recalculate AABB", is_recalculate_bound);
                vertexOffset = EditorGUILayout.Vector3Field("Vertex Offset", vertexOffset);
                vertexRotation = EditorGUILayout.Vector3Field("Vertex Rotation", vertexRotation);
                vertexScale = EditorGUILayout.Vector3Field("Vertex Scale", vertexScale);
                // jave.lin : has_uv0,1,2,3,4,5,6,7
                has_uv0 = EditorGUILayout.Toggle("Has UV0", has_uv0);
                has_uv1 = EditorGUILayout.Toggle("Has UV1", has_uv1);
                has_uv2 = EditorGUILayout.Toggle("Has UV2", has_uv2);
                has_uv3 = EditorGUILayout.Toggle("Has UV3", has_uv3);
                has_uv4 = EditorGUILayout.Toggle("Has UV4", has_uv4);
                has_uv5 = EditorGUILayout.Toggle("Has UV5", has_uv5);
                has_uv6 = EditorGUILayout.Toggle("Has UV6", has_uv6);
                has_uv7 = EditorGUILayout.Toggle("Has UV7", has_uv7);
                // jave.lin : has_color0
                has_color0 = EditorGUILayout.Toggle("Has Color0", has_color0);
                normalImportType = (ModelImporterNormals)EditorGUILayout.EnumPopup("Normal Import Type", normalImportType);
                tangentImportType = (ModelImporterTangents)EditorGUILayout.EnumPopup("Tangent Import Type", tangentImportType);
                semanticMappingType = (SemanticMappingType)EditorGUILayout.EnumPopup("Semantic Mapping Type", semanticMappingType);
                if (semanticMappingType == SemanticMappingType.ManuallyMapping)
                {
                    var refreshCSVSemanticTitle = false;
                    if (GUILayout.Button("Refresh Analysis CSV Semantic Title"))
                    {
                        refreshCSVSemanticTitle = true;
                    }

                    if (semanticManullyMappingTypeDict_key_name_helper == null)
                    {
                        refreshCSVSemanticTitle = true;
                    }

                    if (refreshCSVSemanticTitle)
                    {
                        Analysis_CSV_SemanticTitle();
                    }

                    var keys = semanticManullyMappingTypeDict_key_name_helper.Keys;
                    var stringList = GetStringListHelper();
                    stringList.Clear();
                    stringList.AddRange(keys);

                    // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿喗蓱閸撴艾顭囬幇顔剧＜閻庯綇鎷�?闁归偊鍓涢崝锕€顪冮妶鍡楃瑨閻庢凹鍙冮幃锟犲Ψ閳哄倻鍘介梺鍝勫€圭€笛囧疮閻愮儤鐓熼煫鍥风到濞呭秹鏌″畝鈧崰鏍х暦濞嗘挸围闁糕槄鎷�??
                    stringList.Sort();

                    var check_duplicated_helper = GetSemantics_check_duplicated_helper();
                    for (int i = 0; i < stringList.Count; i++)
                    {
                        if (semanticManullyMappingTypeDict_key_name_helper.TryGetValue(stringList[i], out SemanticType mappedST))
                        {
                            var idx = (int)mappedST;
                            check_duplicated_helper[idx]++;
                        }
                    }

                    // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?婵犵绱曢崑锟�??缂備降鍔庨崕銈囩矉閹烘鏅�? semantic manually mapping data 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鈧箍鍎遍ˇ浼村吹閹达箑绠规繛锝庡墮婵¤偐绱掗悩鑼紞闁瑰弶鐡曠粻娑樷槈濡嚎鍎甸幃妤呮晲鎼粹剝鐏堢紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�? 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�? title
                    EditorGUILayout.BeginHorizontal();
                    {
                        var src_col = GUI.contentColor;
                        GUI.contentColor = Color.yellow;
                        EditorGUILayout.LabelField("CSV Seman Name");
                        useAllComponent = EditorGUILayout.Toggle("閼奉亜濮╅柅澶嬪閹碘偓閺堝鍨庨柌锟�", useAllComponent);
                        useAutoMapping = EditorGUILayout.Toggle("Auto Mapping", useAutoMapping);
                        GUI.contentColor = src_col;
                    }
                    EditorGUILayout.EndHorizontal();

                    // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?婵犵绱曢崑锟�??缂備降鍔庨崕銈囩矉閹烘鏅�? semantic manually mapping data 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鈧箍鍎遍ˇ浼村吹閹达箑绠规繛锝庡墮婵¤偐绱掗悩鑼紞闁瑰弶鐡曠粻娑樷槈濡嚎鍎甸幃妤呮晲鎼粹剝鐏堢紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�?
                    for (int i = 0; i < stringList.Count; i++)
                    {
                        var semantic_name = stringList[i];
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(semantic_name);


                        if (!semanticManullyMappingTypeDict_key_name_helper.TryGetValue(semantic_name, out SemanticType mappedST))
                        {
                            Debug.LogError($"un mapped semantic name : {semantic_name}");
                            continue;
                        }

                        if (useAutoMapping)
                        {
                            mappedST = TryGetSemanticType(semantic_name);
                        }
                        mappedST = (SemanticType)EditorGUILayout.EnumPopup(mappedST);

                        if (useAllComponent)
                        {
                            // 閸婄厧褰夐崠鏍ㄦ
                            if (mappedST != semanticManullyMappingTypeDict_key_name_helper[semantic_name])
                            {
                                SetAttrName(semantic_name, mappedST.ToString());
                                Debug.Log(1);
                            }
                            mappedST = TryGetSemanticType2(semantic_name, mappedST);
                        }


                        semanticManullyMappingTypeDict_key_name_helper[semantic_name] = mappedST;
                        StoreKeyDict();
                        if (check_duplicated_helper[(int)mappedST] > 1)
                        {
                            var src_col = GUI.contentColor;
                            GUI.contentColor = Color.red;
                            EditorGUILayout.LabelField("Duplicated Options");
                            GUI.contentColor = src_col;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    ClearSemantics_check_duplicated_helper(check_duplicated_helper);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌￠敓锟�?闂佹寧绻勯崑娑滅亙闂侀潻鎷�?缂備讲鍋撻柛鏇ㄥ灡閻撳繘鏌涢锝囩畺?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂鍙ョ按濠电姷鏁搁崑娑㈡偋婵犲伅鍝勨堪閸繄顔嗛柣搴嫹?闂佺懓寮堕幐鍐茬暦閻旂⒈鏁�?婵炴潙鍚嬮幑鍥ь潖缂佹ɑ濯撮柤鎭掑劤閵嗗﹪姊洪崫銉バｉ柛鏃€鐟ラ锝夊川婵犲嫮鐦堝┑顕嗘嫹?缂備緤鎷�?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洢鍎茬€氾拷
            EditorGUILayout.Space(10);
            show_mat_toggle = EditorGUILayout.BeginFoldoutHeaderGroup(show_mat_toggle, "Material Options");
            if (show_mat_toggle)
            {
                EditorGUI.indentLevel++;
                var newMaterialSetType = (MaterialSetType)EditorGUILayout.EnumPopup("Material Set Type", materialSetType);
                if (material == null || materialSetType != newMaterialSetType)
                {
                    materialSetType = newMaterialSetType;
                    // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
                    if (materialSetType == MaterialSetType.CreateNew)
                    {
                        // jave.lin : shader 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗蓱閸撴艾顭囬幇顔剧＜閻庯綇鎷�?濠电偞甯楀锟�?闊洦鎸婚崳鐑樼箾閸涱偄鐏叉慨濠冩そ瀹曨偊宕熼鐔蜂壕闁割偅娲栫壕褰掓煛閿燂拷?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
                        if (shader == null)
                        {
                            shader = Shader.Find("Universal Render Pipeline/Lit");
                        }
                        material = new Material(shader);
                    }
                    else
                    {
                        // jave.lin : 濠电媴鎷�?闂佸湱鍎ら幐楣冨煀閺囥垺鐓ラ柡鍥悘鑼偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦纰嶇涵楣冩婢舵劖鐓ユ繝闈涙閸ｆ椽鏌涚€ｃ劌鍔﹂柡灞稿墲閹峰懐鎲撮崟顐わ紦闁诲函鎷�?濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径锟� 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏堢嵁閸℃稑绀�?濡炪們鍨洪悷鈺呭箠閺嶎厼鐓涢柛鎰舵嫹?闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌￠敓锟�?濡ょ姷鍋為〃锟�?闊洦鎸婚ˉ婊堟煛鐎ｎ亞澧�?闁靛骏绲剧涵鐐亜閹存繃鍤�?闊洢鍎茬€氾拷 mat 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
                        var mat_path = Path.Combine(outputDir, fbxName + ".mat").Replace("\\", "/");
                        mat_path = GetAssetPathByFullName(mat_path);
                        var mat_asset = AssetDatabase.LoadAssetAtPath<Material>(mat_path);
                        if (mat_asset != null) material = mat_asset;
                    }
                }

                if (materialSetType == MaterialSetType.CreateNew)
                {
                    // jave.lin : 濠电姷鏁搁崑鐘诲箵椤忓棗绶ら柦姗堟嫹?缁绢參顥撶弧鈧悗纰夋嫹?闂備胶鎳撴晶鐣屽垝椤栫偛纾挎俊銈勯檷娴滄粓鏌熼幍铏珔闁逞屽厵閸婃繂顕ｉ崘娴嬪牚闁割偆鍠撻崢鐢告⒑閸涘﹤鐏熼柛濠冪墱閳ь剨鎷�? shader
                    shader = EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false) as Shader;
                    // jave.lin : 濠电姷鏁搁崑鐘诲箵椤忓棗绶ら柦姗堟嫹?缁绢參顥撶弧鈧悗纰夋嫹?闂備胶鎳撴晶鐣屽垝椤栫偛纾挎俊銈勯檷娴滄粓鏌熼幍铏珔闁逞屽厵閸婃繂顕ｉ崘娴嬪牚闁割偆鍠撻崢鐢告⒑閸涘﹤鐏熼柛濠冪墱閳ь剨鎷�? 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼闂佽崵鍠愬妯衡枔閳哄懏鐓欓柛鎴欏€栫€氾拷
                    texture = EditorGUILayout.ObjectField("Main Texture", texture, typeof(Texture), false) as Texture;
                }
                // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
                else // MaterialSetType.UseExsitMaterialAsset
                {
                    material = EditorGUILayout.ObjectField("Material Asset", material, typeof(Material), false) as Material;
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }


        private void StoreKeyDict()
        {
            semanticManullyMappingTypeDict_Cache.Clear();
            foreach (var item in semanticManullyMappingTypeDict_key_name_helper)
            {
                semanticManullyMappingTypeDict_Cache[item.Key] = item.Value;
            }
        }

        private Dictionary<string, string> attrNameDict;

        private void SetAttrName(string csvAttrName, string semanticName)
        {
            if (attrNameDict == null)
            {
                attrNameDict = new Dictionary<string, string>();
            }
            csvAttrName = csvAttrName.Split('.')[0];
            semanticName = semanticName.Split('_')[0];

            if (!attrNameDict.TryAdd(csvAttrName, semanticName))
            {
                attrNameDict[csvAttrName] = semanticName;
            }
        }

        SemanticType TryGetSemanticType2(string csvAttrName, SemanticType mappedST)
        {
            if (attrNameDict == null)
            {
                return mappedST;
            }

            var csvAttrNames = csvAttrName.Split('.');
            if (csvAttrNames.Length < 2) return mappedST;

            var componentName = csvAttrNames[1].ToUpper();
            if (attrNameDict.TryGetValue(csvAttrNames[0], out string semantic))
            {

                foreach (SemanticType type in Enum.GetValues(typeof(SemanticType)))
                {
                    var typeStr = type.ToString().ToUpper();
                    if (typeStr.Contains(semantic) && typeStr.Contains(componentName))
                    {
                        return type;
                    }
                }
            }
            return mappedST;
        }


        private void Analysis_CSV_SemanticTitle()
        {
            if (semanticManullyMappingTypeDict_key_name_helper != null)
            {
                semanticManullyMappingTypeDict_key_name_helper.Clear();
            }
            else
            {
                semanticManullyMappingTypeDict_key_name_helper = new Dictionary<string, SemanticType>();
            }
            var text = RDC_Text_Asset.text;
            var firstLine = text.Substring(0, text.IndexOf("\n")).Trim();
            var line_element_splitor = new string[] { "," };
            var semanticTitles = firstLine.Split(line_element_splitor, StringSplitOptions.RemoveEmptyEntries);

            MappingSemanticsTypeByNames(ref semanticTypeDict_key_name_helper);

            for (int i = 0; i < semanticTitles.Length; i++)
            {
                var title = semanticTitles[i];
                var semantics = title.Trim();
                if (semanticTypeDict_key_name_helper.TryGetValue(semantics, out SemanticType semanticType))
                {
                    semanticManullyMappingTypeDict_key_name_helper[semantics] = semanticType;
                }
                else
                {
                    // 鐠囪褰囩紓鎾崇摠
                    if (semanticManullyMappingTypeDict_Cache.TryGetValue(semantics, out semanticType))
                    {
                        semanticManullyMappingTypeDict_key_name_helper[semantics] = semanticType;
                    }
                    else
                    {
                        semanticManullyMappingTypeDict_key_name_helper[semantics] = SemanticType.Unknown;
                    }
                }
            }
        }

        private void ExportHandle()
        {
            if (RDC_Text_Asset != null)
            {
                try
                {
                    MappingSemanticsTypeByNames(ref semanticTypeDict_key_name_helper);
                    var parent = GetParentTrans();
                    var outputGO = GameObject.Find($"{GO_Parent_Name}/{fbxName}");
                    if (outputGO != null)
                    {
                        GameObject.DestroyImmediate(outputGO);
                    }
                    outputGO = GenerateGOWithMeshRendererFromCSV(RDC_Text_Asset.text, is_from_DX_CSV);
                    outputGO.transform.SetParent(parent);
                    outputGO.name = fbxName;

                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

                    if (materialSetType == MaterialSetType.CreateNew)
                    {
                        var create_mat = outputGO.GetComponent<MeshRenderer>().sharedMaterial;
                        create_mat.mainTexture = texture;

                        var mat_created_path = Path.Combine(outputDir, fbxName + ".mat").Replace("\\", "/");
                        mat_created_path = GetAssetPathByFullName(mat_created_path);
                        Debug.Log($"mat_created_path : {mat_created_path}");
                        var src_mat = AssetDatabase.LoadAssetAtPath<Material>(mat_created_path);
                        if (src_mat == create_mat)
                        {
                            // nop
                        }
                        else
                        {
                            AssetDatabase.DeleteAsset(mat_created_path);
                            AssetDatabase.CreateAsset(create_mat, mat_created_path);
                        }
                    }

                    ModelExporter.ExportObject(outputFullName, outputGO);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    string mi_path = GetAssetPathByFullName(outputFullName);
                    ModelImporter mi = ModelImporter.GetAtPath(mi_path) as ModelImporter;
                    mi.importNormals = normalImportType;
                    mi.importTangents = tangentImportType;
                    mi.importAnimation = false;
                    mi.importAnimatedCustomProperties = false;
                    mi.importBlendShapeNormals = ModelImporterNormals.None;
                    mi.importBlendShapes = false;
                    mi.importCameras = false;
                    mi.importConstraints = false;
                    mi.importLights = false;
                    mi.importVisibility = false;
                    mi.animationType = ModelImporterAnimationType.None;
                    mi.materialImportMode = ModelImporterMaterialImportMode.None;
                    mi.SaveAndReimport();

                    // jave.lin : replace outputGO from model prefab
                    var src_parent = outputGO.transform.parent;
                    var src_local_pos = outputGO.transform.localPosition;
                    var src_local_rot = outputGO.transform.localRotation;
                    var src_local_scl = outputGO.transform.localScale;
                    DestroyImmediate(outputGO);
                    // jave.lin : new model prefab
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(mi_path);
                    outputGO = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    outputGO.transform.SetParent(src_parent);
                    outputGO.transform.localPosition = src_local_pos;
                    outputGO.transform.localRotation = src_local_rot;
                    outputGO.transform.localScale = src_local_scl;
                    outputGO.name = fbxName;
                    // jave.lin : set material
                    var mat_path = Path.Combine(outputDir, fbxName + ".mat").Replace("\\", "/");
                    mat_path = GetAssetPathByFullName(mat_path);
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(mat_path);
                    outputGO.GetComponent<MeshRenderer>().sharedMaterial = mat;
                    // jave.lin : new real prefab
                    var prefab_created_path = Path.Combine(outputDir, fbxName + ".prefab").Replace("\\", "/");
                    prefab_created_path = GetAssetPathByFullName(prefab_created_path);
                    Debug.Log($"prefab_created_path : {prefab_created_path}");
                    PrefabUtility.SaveAsPrefabAssetAndConnect(outputGO, prefab_created_path, InteractionMode.AutomatedAction);

                    // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂佽娴烽幊鎾垛偓姘煎幖椤灝螣閼测晝鐒惧銈呯箰閻楀﹪鍩涢幋锔藉仯闁诡厽甯掓俊濂告煛鐎ｎ偄鐏撮柡宀嬬磿閳ь剨鎷�?濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃锟�??闂佸搫琚崕杈╃不閾忣偂绻嗛柕鍫濆閸斿秵绻涢崨顐㈢伈婵﹥妞藉畷顐﹀礋椤愮喎浜鹃柛顐ｆ礀绾惧綊鏌￠敓锟�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夐崕鎶芥偤閵娧呯闁搞儺鍓氶悡鏇㈡煙閿燂拷?闁诲海鐟抽崨顏勪壕婵鍋撶€氾拷
                    Debug.Log($"Export FBX Successfully! outputPath : {outputFullName}");
                }
                catch (Exception er)
                {
                    Debug.LogError($"Export FBX Failed! er: {er}");
                }
            }
        }

        // jave.lin : 闂傚倸鍊风粈渚€骞栭銈傚亾閿燂拷?濠电姷鏁搁崑鎰板磻閹剧粯鈷戦弶鐐村閸斿秹鏌熼悷鐗堟悙??濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径锟� semantics 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�? name 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�? type
        private void MappingSemanticsTypeByNames(ref Dictionary<string, SemanticType> container)
        {
            if (container == null)
            {
                container = new Dictionary<string, SemanticType>();
            }
            else
            {
                container.Clear();
            }
            container["VTX"] = SemanticType.VTX;
            container["IDX"] = SemanticType.IDX;
            container["SV_POSITION.x"] = SemanticType.POSITION_X;
            container["SV_POSITION.y"] = SemanticType.POSITION_Y;
            container["SV_POSITION.z"] = SemanticType.POSITION_Z;
            container["SV_POSITION.w"] = SemanticType.POSITION_W;
            container["SV_Position.x"] = SemanticType.POSITION_X;
            container["SV_Position.y"] = SemanticType.POSITION_Y;
            container["SV_Position.z"] = SemanticType.POSITION_Z;
            container["SV_Position.w"] = SemanticType.POSITION_W;
            container["POSITION.x"] = SemanticType.POSITION_X;
            container["POSITION.y"] = SemanticType.POSITION_Y;
            container["POSITION.z"] = SemanticType.POSITION_Z;
            container["POSITION.w"] = SemanticType.POSITION_W;
            container["NORMAL.x"] = SemanticType.NORMAL_X;
            container["NORMAL.y"] = SemanticType.NORMAL_Y;
            container["NORMAL.z"] = SemanticType.NORMAL_Z;
            container["NORMAL.w"] = SemanticType.NORMAL_W;
            container["TANGENT.x"] = SemanticType.TANGENT_X;
            container["TANGENT.y"] = SemanticType.TANGENT_Y;
            container["TANGENT.z"] = SemanticType.TANGENT_Z;
            container["TANGENT.w"] = SemanticType.TANGENT_W;
            container["TEXCOORD0.x"] = SemanticType.TEXCOORD0_X;
            container["TEXCOORD0.y"] = SemanticType.TEXCOORD0_Y;
            container["TEXCOORD0.z"] = SemanticType.TEXCOORD0_Z;
            container["TEXCOORD0.w"] = SemanticType.TEXCOORD0_W;
            container["TEXCOORD1.x"] = SemanticType.TEXCOORD1_X;
            container["TEXCOORD1.y"] = SemanticType.TEXCOORD1_Y;
            container["TEXCOORD1.z"] = SemanticType.TEXCOORD1_Z;
            container["TEXCOORD1.w"] = SemanticType.TEXCOORD1_W;
            container["TEXCOORD2.x"] = SemanticType.TEXCOORD2_X;
            container["TEXCOORD2.y"] = SemanticType.TEXCOORD2_Y;
            container["TEXCOORD2.z"] = SemanticType.TEXCOORD2_Z;
            container["TEXCOORD2.w"] = SemanticType.TEXCOORD2_W;
            container["TEXCOORD3.x"] = SemanticType.TEXCOORD3_X;
            container["TEXCOORD3.y"] = SemanticType.TEXCOORD3_Y;
            container["TEXCOORD3.z"] = SemanticType.TEXCOORD3_Z;
            container["TEXCOORD3.w"] = SemanticType.TEXCOORD3_W;
            container["TEXCOORD4.x"] = SemanticType.TEXCOORD4_X;
            container["TEXCOORD4.y"] = SemanticType.TEXCOORD4_Y;
            container["TEXCOORD4.z"] = SemanticType.TEXCOORD4_Z;
            container["TEXCOORD4.w"] = SemanticType.TEXCOORD4_W;
            container["TEXCOORD5.x"] = SemanticType.TEXCOORD5_X;
            container["TEXCOORD5.y"] = SemanticType.TEXCOORD5_Y;
            container["TEXCOORD5.z"] = SemanticType.TEXCOORD5_Z;
            container["TEXCOORD5.w"] = SemanticType.TEXCOORD5_W;
            container["TEXCOORD6.x"] = SemanticType.TEXCOORD6_X;
            container["TEXCOORD6.y"] = SemanticType.TEXCOORD6_Y;
            container["TEXCOORD6.z"] = SemanticType.TEXCOORD6_Z;
            container["TEXCOORD6.w"] = SemanticType.TEXCOORD6_W;
            container["TEXCOORD7.x"] = SemanticType.TEXCOORD7_X;
            container["TEXCOORD7.y"] = SemanticType.TEXCOORD7_Y;
            container["TEXCOORD7.z"] = SemanticType.TEXCOORD7_Z;
            container["TEXCOORD7.w"] = SemanticType.TEXCOORD7_W;
            container["COLOR0.x"] = SemanticType.COLOR0_X;
            container["COLOR0.y"] = SemanticType.COLOR0_Y;
            container["COLOR0.z"] = SemanticType.COLOR0_Z;
            container["COLOR0.w"] = SemanticType.COLOR0_W;
            container["COLOR.x"] = SemanticType.COLOR0_X;
            container["COLOR.y"] = SemanticType.COLOR0_Y;
            container["COLOR.z"] = SemanticType.COLOR0_Z;
            container["COLOR.w"] = SemanticType.COLOR0_W;
        }

        // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂佽娴烽幊鎾垛偓姘煎幖椤灝螣閻撳海绛忛梺鍖℃嫹? parent transform 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
        private Transform GetParentTrans()
        {
            var parentGO = GameObject.Find(GO_Parent_Name);
            if (parentGO == null)
            {
                parentGO = new GameObject(GO_Parent_Name);
                parentGO.transform.position = Vector3.zero;
                parentGO.transform.localRotation = Quaternion.identity;
                parentGO.transform.localScale = Vector3.one;
            }
            return parentGO.transform;
        }

        // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 GO Name
        private string GenerateGOName(TextAsset ta)
        {
            //return $"From_CSV_{ta.text.GetHashCode()}";
            //return $"From_CSV_{ta.name}";
            return ta.name;
        }

        // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 CSV 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洢鍎茬€氾拷 MeshRenderer 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂佽娴烽幊鎾垛偓姘煎幗缁旂喖宕卞▎灞戒壕闁荤噦鎷�?闁归偊鍘搁幏娲煟閻樺厖鑸柛鏂跨焸瀵悂骞嬮敂鐣屽幐闁诲函鎷�?濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟﹀▍銏ゆ⒑鐠恒劌娅愰柟鍑ゆ嫹 GO
        private GameObject GenerateGOWithMeshRendererFromCSV(string csv, bool is_from_DX_CSV)
        {
            var ret = new GameObject();

            var mesh = new Mesh();

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 csv 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍鐐烘⒑缁嬫寧婀扮紒瀣灴椤拷? mesh 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倸鍊风粈渚€鎮樺┑瀣垫晞闁告洦鍘介～鏇㈡煥閿燂拷?
            FillMeshFromCSV(mesh, csv, is_from_DX_CSV);

            var meshFilter = ret.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = ret.AddComponent<MeshRenderer>();

            // jave.lin : 濠电媴鎷�?闂佸湱鍎ら幐楣冨煀閺囥垺鐓ラ柡鍥悘鑼偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦纰嶇涵楣冩婢舵劖鐓ユ繝闈涙閸ｆ椽鏌涚€ｃ劌鍔﹂柡灞稿墲閹峰懐鎲撮崟顐わ紦闁诲函鎷�?濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径锟� URP 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�? PBR Shader
            meshRenderer.sharedMaterial = material;

            ret.transform.position = Vector3.zero;
            ret.transform.localRotation = Quaternion.identity;
            ret.transform.localScale = Vector3.one;

            return ret;
        }

        // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 semantic type 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�? data 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗枪妞存悂宕甸埀顒勬⒑鐎圭姵顥�?闁归偊鍘奸埀顒€顭烽弻娑㈠Ψ閿濆懎顬堝銈忔嫹? 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊冣攽閻愯泛鐨虹紒顕呭灦瀵拷?闂傚倷娴囧畷鍨叏閸愯褰掑炊椤掑﹦绋忔繝銏ｆ硾婢跺洭宕戦幘鎰佹僵闁绘劦鍓欓锟�
        private void FillVertexFieldInfo(VertexInfo info, SemanticType semanticType, string data, bool is_from_DX_CSV)
        {
            switch (semanticType)
            {
                // jave.lin : VTX
                case SemanticType.VTX:
                    info.VTX = int.Parse(data);
                    break;

                // jave.lin : IDX
                case SemanticType.IDX:
                    info.IDX = int.Parse(data);
                    break;

                // jave.lin : position
                case SemanticType.POSITION_X:
                    info.POSITION_X = float.Parse(data);
                    break;
                case SemanticType.POSITION_Y:
                    info.POSITION_Y = float.Parse(data);
                    break;
                case SemanticType.POSITION_Z:
                    info.POSITION_Z = float.Parse(data);
                    break;
                case SemanticType.POSITION_W:
                    info.POSITION_W = float.Parse(data);
                    Debug.LogWarning("WARNING: unity mesh cannot transfer position.w to shader program.");
                    break;

                // jave.lin : normal
                case SemanticType.NORMAL_X:
                    info.NORMAL_X = float.Parse(data);
                    break;
                case SemanticType.NORMAL_Y:
                    info.NORMAL_Y = float.Parse(data);
                    break;
                case SemanticType.NORMAL_Z:
                    info.NORMAL_Z = float.Parse(data);
                    break;
                case SemanticType.NORMAL_W:
                    info.NORMAL_W = float.Parse(data);
                    break;

                // jave.lin : tangent
                case SemanticType.TANGENT_X:
                    info.TANGENT_X = float.Parse(data);
                    break;
                case SemanticType.TANGENT_Y:
                    info.TANGENT_Y = float.Parse(data);
                    break;
                case SemanticType.TANGENT_Z:
                    info.TANGENT_Z = float.Parse(data);
                    break;
                case SemanticType.TANGENT_W:
                    info.TANGENT_W = float.Parse(data);
                    break;

                // jave.lin : texcoord0
                case SemanticType.TEXCOORD0_X:
                    info.TEXCOORD0_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD0_Y:
                    info.TEXCOORD0_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD0_Y = 1 - info.TEXCOORD0_Y;
                    break;
                case SemanticType.TEXCOORD0_Z:
                    info.TEXCOORD0_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD0_W:
                    info.TEXCOORD0_W = float.Parse(data);
                    break;

                // jave.lin : texcoord1
                case SemanticType.TEXCOORD1_X:
                    info.TEXCOORD1_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD1_Y:
                    info.TEXCOORD1_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD1_Y = 1 - info.TEXCOORD1_Y;
                    break;
                case SemanticType.TEXCOORD1_Z:
                    info.TEXCOORD1_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD1_W:
                    info.TEXCOORD1_W = float.Parse(data);
                    break;

                // jave.lin : texcoord2
                case SemanticType.TEXCOORD2_X:
                    info.TEXCOORD2_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD2_Y:
                    info.TEXCOORD2_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD2_Y = 1 - info.TEXCOORD2_Y;
                    break;
                case SemanticType.TEXCOORD2_Z:
                    info.TEXCOORD2_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD2_W:
                    info.TEXCOORD2_W = float.Parse(data);
                    break;

                // jave.lin : texcoord3
                case SemanticType.TEXCOORD3_X:
                    info.TEXCOORD3_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD3_Y:
                    info.TEXCOORD3_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD3_Y = 1 - info.TEXCOORD3_Y;
                    break;
                case SemanticType.TEXCOORD3_Z:
                    info.TEXCOORD3_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD3_W:
                    info.TEXCOORD3_W = float.Parse(data);
                    break;

                // jave.lin : texcoord4
                case SemanticType.TEXCOORD4_X:
                    info.TEXCOORD4_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD4_Y:
                    info.TEXCOORD4_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD4_Y = 1 - info.TEXCOORD4_Y;
                    break;
                case SemanticType.TEXCOORD4_Z:
                    info.TEXCOORD4_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD4_W:
                    info.TEXCOORD4_W = float.Parse(data);
                    break;

                // jave.lin : texcoord5
                case SemanticType.TEXCOORD5_X:
                    info.TEXCOORD5_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD5_Y:
                    info.TEXCOORD5_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD5_Y = 1 - info.TEXCOORD5_Y;
                    break;
                case SemanticType.TEXCOORD5_Z:
                    info.TEXCOORD5_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD5_W:
                    info.TEXCOORD5_W = float.Parse(data);
                    break;

                // jave.lin : texcoord6
                case SemanticType.TEXCOORD6_X:
                    info.TEXCOORD6_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD6_Y:
                    info.TEXCOORD6_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD6_Y = 1 - info.TEXCOORD6_Y;
                    break;
                case SemanticType.TEXCOORD6_Z:
                    info.TEXCOORD6_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD6_W:
                    info.TEXCOORD6_W = float.Parse(data);
                    break;

                // jave.lin : texcoord7
                case SemanticType.TEXCOORD7_X:
                    info.TEXCOORD7_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD7_Y:
                    info.TEXCOORD7_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD7_Y = 1 - info.TEXCOORD7_Y;
                    break;
                case SemanticType.TEXCOORD7_Z:
                    info.TEXCOORD7_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD7_W:
                    info.TEXCOORD7_W = float.Parse(data);
                    break;

                // jave.lin : color0
                case SemanticType.COLOR0_X:
                    info.COLOR0_X = float.Parse(data);
                    break;
                case SemanticType.COLOR0_Y:
                    info.COLOR0_Y = float.Parse(data);
                    break;
                case SemanticType.COLOR0_Z:
                    info.COLOR0_Z = float.Parse(data);
                    break;
                case SemanticType.COLOR0_W:
                    info.COLOR0_W = float.Parse(data);
                    break;
                case SemanticType.Unknown:
                    // jave.lin : nop
                    break;
                // jave.lin : un-implements
                default:
                    Debug.LogError($"Fill_A2V_Common_Type_Data un-implements SemanticType : {semanticType}");
                    break;
            }
        }

        // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 csv 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍鐐烘⒑缁嬫寧婀扮紒瀣灴椤拷? mesh 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倸鍊风粈渚€鎮樺┑瀣垫晞闁告洦鍘介～鏇㈡煥閿燂拷?
        private void FillMeshFromCSV(Mesh mesh, string csv, bool is_from_DX_CSV)
        {
            var line_splitor = new string[] { "\n" };
            var line_element_splitor = new string[] { "," };

            var lines = csv.Split(line_splitor, StringSplitOptions.RemoveEmptyEntries);

            // jave.lin : lines[0] == "VTX, IDX, POSITION.x, POSITION.y, POSITION.z, NORMAL.x, NORMAL.y, NORMAL.z, NORMAL.w, TANGENT.x, TANGENT.y, TANGENT.z, TANGENT.w, TEXCOORD0.x, TEXCOORD0.y"

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 vertex buffer format 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�? semantics 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�? idx 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鈧箍鍎遍ˇ浼村吹閹达箑绠归弶鍫濆⒔缁嬭鈹戦姘ュ仮闁诡喖鍢查埢搴ょ疀閿燂拷??婵＄偑鍊曠换鎰版偉婵傚摜宓侀柡宥庡亐閸嬫挸鈽夊▍铏灴钘熼柛顐犲劜閻撴稑霉閿濆牜娼愮€规洖鐬奸埀顒婃嫹?濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕磽閸屾艾鈧娆㈤敓鐘插瀭濞寸姴顑呯粻顖炴煥閿燂拷?
            var semanticTitles = lines[0].Split(line_element_splitor, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, SemanticType> semantic_type_map_key_name;
            if (semanticMappingType == SemanticMappingType.Default)
            {
                semantic_type_map_key_name = semanticTypeDict_key_name_helper;
            }
            else
            {
                semantic_type_map_key_name = semanticManullyMappingTypeDict_key_name_helper;
            }

            semanticsIDX_helper = new SemanticType[semanticTitles.Length];
            Debug.Log($"semanticTitles : {lines[0]}");
            for (int i = 0; i < semanticTitles.Length; i++)
            {
                var title = semanticTitles[i];
                var semantics = title.Trim();
                if (semantic_type_map_key_name.TryGetValue(semantics, out SemanticType semanticType))
                {
                    semanticsIDX_helper[i] = semanticType;
                    //Debug.Log($"semantics : {semantics}, type : {semanticType}");
                }
                else
                {
                    Debug.LogWarning($"un-implements semantic : {semantics}");
                }
            }

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌￠敓锟�?婵犲痉銈嗙【閻撱倖銇勮箛鎾村婵☆偄鍟村娲礃閸欏鍎撻梺绋匡攻閹拷?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟﹀▍銏ゆ⒑鐠恒劌娅愰柟鍑ゆ嫹 IDX 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼闂佽崵鍠愬妯何ｆ繝姘厱闁靛骏绲介惃娲煛閿燂拷?闂備浇娉曢崳锕傚箯閿燂拷 vertex buffer 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼闂佽崵鍠愬妯衡枔閳哄懏鐓欓柛鎴欏€栫€氾拷
            // lines[1~count-1] : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挾鍠栭弻銊モ攽閸℃ê娅ч梺缁樺笩濞夋洜妲愰幒妤€绠熼悗锝庡亜椤忥拷 0, 0,  0.0402, -1.57095E-17,  0.12606, -0.97949,  0.00, -0.20056,  0.00,  0.1098,  0.83691, -0.53613,  1.00, -0.06058,  0.81738

            Dictionary<int, VertexInfo> vertex_dict_key_idx = new Dictionary<int, VertexInfo>();

            var indices = new List<int>();

            var min_idx = int.MaxValue;
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var linesElements = line.Split(line_element_splitor, StringSplitOptions.RemoveEmptyEntries);

                // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌￠崶鈺佹瀺缂佽妫欓妵鍕箻鐠虹洅锝夋煕韫囨梻鐭掓慨濠冩そ瀹曪拷?濡炪倖鍨甸幊锟�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹0~count-1)
                var idx = int.Parse(linesElements[1]);
                if (min_idx > idx)
                {
                    min_idx = idx;
                }
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var linesElements = line.Split(line_element_splitor, StringSplitOptions.RemoveEmptyEntries);

                // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌￠崶鈺佹瀺缂佽妫欓妵鍕箻鐠虹洅锝夋煕韫囨梻鐭掓慨濠冩そ瀹曪拷?濡炪倖鍨甸幊锟�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹0~count-1)
                var idx = int.Parse(linesElements[1]) - min_idx;

                // jave.lin : indices 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏呭仭闁规鍠氭婵＄偑鍊戦崕鏌ュ箲閸ヮ剙违闁稿矉鎷�?闁哄洦顨呮禍鎯旈悩闈涗粶缂佺粯锕㈤獮鍐ㄢ堪閸喎娈熼梺闈涱檧闂勶拷?闁靛繈鍊栭埛鎴︽煕濠靛棗顏╅柍褜鍓欓悥鐓庣暦閹版澘鍐€?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼闂佽崵鍠愬妯衡枔閳哄懏鐓欓柛鎴欏€栫€氾拷
                indices.Add(idx);

                // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍鐐烘⒑缁嬫寧婀扮紒瀣灴椤拷? vertex 婵犵數濮烽弫鎼佸磻濞戙垺鍎楀璺烘湰瀹曞弶绻濋棃娑卞剰闁哄绶氶弻鐔煎箲閿燂拷?闁肩⒈鍓涢崢顒佷繆閻愵亜鈧牠宕濋幋锕€纾块柤鑹扮堪娴滃湱鎲搁弮鍫濊摕闁绘棑鎷�?閻庯綆浜妤呮煕濡粯灏﹂柡灞稿墲閹峰懐鎲撮崟顐わ紦闁诲函鎷�?濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕⒒閸屾艾鈧嘲霉閸パ€鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼闂侀潻鎷�?闂佸搫鑻ˇ顖炲Υ閹烘鎹�?缂備緡鍠栭…鐑藉箖濞嗘挻鍊绘俊顖滃帶瀵娊姊绘担铏瑰笡闁哄被鍔戦獮澶愭晸閻樺啿鈧埖銇勯弴妤€浜鹃梺鍝勭灱閸犲酣鎮鹃敓鐘茬骇闁圭ǹ宸╅埡浣勬棃鎮╅棃娑楁勃闂佸湱鎳撳ú顓烆嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洢鍎茬€氾拷
                if (!vertex_dict_key_idx.TryGetValue(idx, out VertexInfo info))
                {
                    info = new VertexInfo();
                    vertex_dict_key_idx[idx] = info;

                    // jave.lin : loop to fill the a2v field
                    for (int j = 0; j < linesElements.Length; j++)
                    {
                        var semanticType = semanticsIDX_helper[j];
                        FillVertexFieldInfo(info, semanticType, linesElements[j], is_from_DX_CSV);
                    }
                }
            }

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挾濞€閹鈽夊▍顓т邯閹偟鎲撮崟顏嗙畾闂佸湱绮敮锟�?濞寸厧顕畵渚€鐓崶銊р姇闁稿﹤顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿喗蓱閸撴艾顭囬幇顓犵缁炬澘褰夐柇顖涖亜閵忊槅娈滅€规洜鍠栭、妤佹媴閿燂拷?闁靛繈鍊栭埛鎴︽煕濠靛棗顏╅柍褜鍓欓悥鐓庣暦閹版澘鍐€?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏堝箖娴兼惌鏁嬮柍褜鍓熼悰顔芥償閵婏箑娈熼梺闈涱檧闂勶拷?闁靛繈鍊栭埛鎴︽煕濠靛棗顏╅柍褜鍓欓悥鐓庣暦閹版澘鍐€?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
            var rotation = Quaternion.Euler(vertexRotation);
            var TRS_mat = Matrix4x4.TRS(vertexOffset, rotation, vertexScale);
            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌涘┑鍕姢缁炬儳銈搁弻锟犲礃閳轰胶浠村Δ鐘靛仦閻楁粓骞夐幖浣哥骇婵炲棛鍋撻埢鍫澪旈悩闈涗沪闁搞劍瀵ч幈銊╁焵椤掑嫭鐓忛柛顐ｇ箖椤ョ偞绻涢崨顐㈢伈婵﹥妞藉畷顐﹀礋椤愮喎浜鹃柛顐ｆ礀绾惧綊鏌￠敓锟�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝夋煙椤旀儳鍘寸€殿喗娼欓～婵囨綇閳哄啫搴婇梻鍌欐祰濡椼劑鎮為敃鍌氱闁搞儻鎷�??濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕⒒閸屾艾鈧嘲霉閸パ€鏋栭柡鍥ュ灩缁愭鏌￠崶锝嗩潑闁哄棙婢橀埞鎴︽偐椤旇偐浼囧┑鐐差槹缁嬫捇鍩€椤掍胶鐓柛妤€鍟块锟�?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍鐐烘⒑缁嬫寧婀扮紒瀣灴椤拷? vertex scale 濠电姷鏁搁崑鐐哄垂閸洖绠伴悹鍥锋嫹?闁绘劖澹嗛惌娆戔偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洢鍎茬€氾拷 uniform scale 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍鐐烘⒑缁嬫寧婀扮紒瀣灴椤拷?
            // ref : LearnGL - 11.5 - 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹04 - 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌涘┑鍕姢缁炬儳銈搁弻锟犲礃閳轰胶浠村Δ鐘靛仦鐢偟妲愰幒妤婃晩闁诡垼鐏濋妷鈺傜厵鐎瑰嫸鎷�??闂佺懓寮堕幐鍐茬暦閻旂⒈鏁�?婵炴潙鍚嬮幑鍥ь潖缂佹ɑ濯撮柤鎭掑劤閵嗗﹪姊洪崫銉バｉ柛鏃€鐟ラ锝夊川婵犲嫮鐦堝┑顕嗘嫹?缂備緤鎷�?闂佽娴烽幊鎾垛偓姘煎幖椤灝螣閼测晝骞撳┑掳鍊曢幊蹇涘煕閹达附鍋ｉ柟顓熷笒婵″ジ鏌＄€ｎ偄鐏撮柡灞剧洴婵℃悂鏁傞崜褏鏉芥俊銈囧Х閸嬬偤宕归幐搴㈠弿闁逞屽墴閺屾洟宕煎┑鍥舵！濠电偛鎳岄崹钘夘潖濞差亜浼犻柛鏇ㄥ亐閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝夋煛鐏炲墽鈽夐摶鏍煕閹扳晛濡烽柡瀣嚇濮婄儤娼幍顔煎闂佸湱鎳撳ú顓烆嚕椤愶箑绠荤紓浣股戝▍銏ゆ⒑鐠恒劌娅愰柟鍑ゆ嫹
            // https://blog.csdn.net/linjf520/article/details/107501215
            var M_IT_mat = Matrix4x4.TRS(Vector3.zero, rotation, vertexScale).inverse.transpose;

            // jave.lin : composite the data 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓璺ㄢ偓纰夋嫹?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?濠电姭鍋撻梺顒€绉寸粻鐔兼煟濡も偓閻楀繘鎮疯ぐ鎺撶厓鐟滄粓宕滈悢濂夊殨?闂備浇顫夊畷锟�?闁告瑱鎷�?婵せ鍋撻柡灞炬礋瀹曠厧鈹戦敓锟�?婵°倧鎷�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂閿亾妤ｅ啯鈷戦柛婵撴嫹??濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ラ埀顒€顭烽弻銈夊箒閹烘垵濮曞┑鐐叉噷閸ㄨ棄顫忓ú顏勪紶闁告洦鍋€閸嬫捇宕奸弴鐐碉紮闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏堢嵁婵犲洦鏅搁柣妯垮皺閸樻悂姊洪崨濠佺繁闁哥姵顨呴埢宥夊閵堝棌鎷绘繛杈剧悼閹筹拷?闁糕剝绋戦弸浣糕攽閻樺磭顣查柛瀣戞穱锟�?闁荤姴娲ゅΟ濠囧吹閵堝應鏀介柣鎰级椤ョ偤鏌涢弮鈧ú锟�?闊洦鎸鹃崺锝嗘叏婵犲懏顏犻柟鐟板婵℃悂濡烽敂鍙ョ敖濠碉紕鍋戦崐鎴﹀垂閾忕懓鍨濋幖娣妼缁愭鈹戦悩鎻掓殭缂佸墎鍋ら幃妤呮晲鎼存繃鍠氭繛鏉戝悑閹瑰洤顫忕紒妯诲闁兼亽鍎抽妴濠囨⒑闂堚晝绉剁紒鐘虫崌閻涱喛绠涘☉娆愭闂佸尅鎷�? mesh闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�?
            var vertices = new Vector3[vertex_dict_key_idx.Count];
            var normals = new Vector3[vertex_dict_key_idx.Count];
            var tangents = new Vector4[vertex_dict_key_idx.Count];
            var uv = new Vector2[vertex_dict_key_idx.Count];
            var uv2 = new Vector2[vertex_dict_key_idx.Count];
            var uv3 = new Vector2[vertex_dict_key_idx.Count];
            var uv4 = new Vector2[vertex_dict_key_idx.Count];
            var uv5 = new Vector2[vertex_dict_key_idx.Count];
            var uv6 = new Vector2[vertex_dict_key_idx.Count];
            var uv7 = new Vector2[vertex_dict_key_idx.Count];
            var uv8 = new Vector2[vertex_dict_key_idx.Count];
            var color0 = new Color[vertex_dict_key_idx.Count];

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 0~count 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿噯鎷�?缂傚倸绉撮敃顏勵嚕婵犳艾惟闁冲搫鍊告禍婊堟⒑閸涘﹦鈽夐柨鏇檮鐎靛ジ宕熼娑掓嫼閻熸粎澧楃敮鈺佄涢幋锔界厱闊洦妫戦懓鍨攽閳ュ磭鎽犻柟顖涙婵℃悂鏁傞幆褍姹查梻鍌欒兌缁垶寮婚妸鈺佺疅闁跨喓濮甸崐鑸点亜閺囨浜鹃梺鍝勭灱閸狅拷?閻庯綆浜妤呮煕濡粯灏﹂柡灞稿墲閹峰懐鎲撮崟顐わ紦闁诲函鎷�?濡炪倖甯掔€氼剙娲块梻浣虹《閸擄拷?濠电姳鑳舵径鍕⒒閸屾艾鈧嘲霉閸パ€鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?婵犵绱曢崑锟�??濠电偛顕崗姗€骞嗗畝鍐杸婵炴垶鐟㈤幏娲煟閻樺厖鑸柛鏂跨焸瀵悂骞嬮敂鐣屽幐闁诲函鎷�?濡炪値鍘鹃崗姗€鎮伴璺ㄧ杸婵炴垶鐟ч崣鍡涙煟鎼搭垳绉甸柛鎾村哺閹線寮崼鐔叉嫽婵炶揪缍€婵倗娑甸崼鏇熺厱闁挎繂绻掗悾鍨殽閻愯尙绠婚柡浣规崌閺侊拷? vertex 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹
            for (int idx = 0; idx < vertices.Length; idx++)
            {
                var info = vertex_dict_key_idx[idx];
                vertices[idx] = TRS_mat * info.POSITION_H;
                normals[idx] = M_IT_mat * info.NORMAL;
                tangents[idx] = info.TANGENT;
                uv[idx] = info.TEXCOORD0;
                uv2[idx] = info.TEXCOORD1;
                uv3[idx] = info.TEXCOORD2;
                uv4[idx] = info.TEXCOORD3;
                uv5[idx] = info.TEXCOORD4;
                uv6[idx] = info.TEXCOORD5;
                uv7[idx] = info.TEXCOORD6;
                uv8[idx] = info.TEXCOORD7;
                color0[idx] = info.COLOR0;
            }

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻鐩弻娑氫沪娑斿拋浜崺鈧�?闂佺尨鎷�?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 mesh 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倸鍊风粈渚€鎮樺┑瀣垫晞闁告洦鍘介～鏇㈡煥閿燂拷?
            mesh.vertices = vertices;

            // jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌￠敓锟�?闂佹寧绻勯崑娑滅亙闂侀潻鎷�?缂備讲鍋撻柛鏇ㄥ灡閻撳繘鏌涢锝囩畺?婵°倧鎷�?闊洢鍎茬€氾拷 reverse idx
            if (is_reverse_vertex_order) indices.Reverse();
            mesh.triangles = indices.ToArray();

            // jave.lin : unity 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂傚倷鑳剁划顖炲垂閸忓吋鍙忛柕鍫濐槸閻ら箖鏌熼梻瀵稿妽闁绘挻绋戦…璺ㄦ崉閿燂拷??濡炪們鍎遍鍡涘Φ閸曨垰绠抽柟鎹愭硾瀵劌螖閻橀潧浠滅紒缁橈耿楠炲啫饪伴崼鐔锋疅闂侀潧顧€闂勶拷?闁靛繈鍊栭埛鎴︽煕濠靛棗顏╅柍褜鍓欓悥鐓庣暦閹版澘鍐€?闂佺鎷�?濠殿喗顭囬崑鎾垛偓姘炬嫹 uv[0~7]
            mesh.uv = has_uv0 ? uv : null;
            mesh.uv2 = has_uv1 ? uv2 : null;
            mesh.uv3 = has_uv2 ? uv3 : null;
            mesh.uv4 = has_uv3 ? uv4 : null;
            mesh.uv5 = has_uv4 ? uv5 : null;
            mesh.uv6 = has_uv5 ? uv6 : null;
            mesh.uv7 = has_uv6 ? uv7 : null;
            mesh.uv8 = has_uv7 ? uv8 : null;

            mesh.colors = has_color0 ? color0 : null;

            // jave.lin : AABB
            if (is_recalculate_bound)
            {
                mesh.RecalculateBounds();
            }

            // jave.lin : NORMAL
            switch (normalImportType)
            {
                case ModelImporterNormals.None:
                    // nop
                    break;
                case ModelImporterNormals.Import:
                    mesh.normals = normals;
                    break;
                case ModelImporterNormals.Calculate:
                    mesh.RecalculateNormals();
                    break;
                default:
                    break;
            }

            // jave.lin : TANGENT
            switch (tangentImportType)
            {
                case ModelImporterTangents.None:
                    // nop
                    break;
                case ModelImporterTangents.Import:
                    mesh.tangents = tangents;
                    break;
                case ModelImporterTangents.CalculateLegacy:
                case ModelImporterTangents.CalculateLegacyWithSplitTangents:
                case ModelImporterTangents.CalculateMikk:
                    mesh.RecalculateTangents();
                    break;
                default:
                    break;
            }

            //// jave.lin : 闂傚倸鍊搁崐宄懊归崶褉鏋栭柡鍥ュ灩缁愭鏌熼悧鍫熺凡闁告垹濮撮埞鎴︽偐鐎圭姴顥濈紓渚婃嫹?闂佽娴烽幊鎾垛偓姘煎幖椤灝螣閼测晝鐒惧銈呯箰鐎氀兾ｉ崼銉︾厵缂備降鍨归悘鐘裁瑰⿰鍫㈢暫婵﹦绮幏鍛存嚍閵壯佲偓濠囨⒑閸濄儱校闁告梹鐟ラ锝夊川婵犲嫮鐦堝┑顕嗘嫹?缂備緤鎷�?闂傚倷娴囧▔鏇㈠闯閿曞倸绠�?
            //Debug.Log("FillMeshFromCSV done!");
        }
    }


#endif

}

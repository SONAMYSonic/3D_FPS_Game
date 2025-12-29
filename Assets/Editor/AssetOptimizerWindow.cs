#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Unity 프로젝트 용량 최적화 도구입니다.
/// 
/// ───────────────────────────────────────────────────────────
/// 📚 사용 방법:
/// ───────────────────────────────────────────────────────────
/// 
/// 1. Unity 메뉴 → Tools → Asset Optimizer (용량 최적화) 선택
/// 2. "프로젝트 용량 분석" 버튼으로 현재 상태 확인
/// 3. 원하는 최적화 버튼 클릭!
/// 
/// ───────────────────────────────────────────────────────────
/// </summary>
public class AssetOptimizerWindow : EditorWindow
{
    // ═══════════════════════════════════════════════════════════
    // 설정값
    // ═══════════════════════════════════════════════════════════
    
    private int _maxTextureSize = 1024;
    private bool _useCrunchCompression = true;
    private int _crunchQuality = 75;
    private bool _generateMipmaps = true;
    
    private bool _meshCompression = true;
    private bool _readWriteDisabled = true;
    private bool _optimizeMesh = true;
    
    private Vector2 _scrollPosition;
    private string _logOutput = "";

    // ═══════════════════════════════════════════════════════════
    // 메뉴 등록
    // ═══════════════════════════════════════════════════════════
    
    [MenuItem("Tools/Asset Optimizer (용량 최적화)")]
    public static void ShowWindow()
    {
        var window = GetWindow<AssetOptimizerWindow>("Asset Optimizer");
        window.minSize = new Vector2(400, 600);
    }

    // ═══════════════════════════════════════════════════════════
    // GUI 렌더링
    // ═══════════════════════════════════════════════════════════
    
    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        DrawHeader();
        DrawAnalysisSection();
        DrawTextureSection();
        DrawModelSection();
        DrawAudioSection();
        DrawBatchOptimizeSection();
        DrawLogSection();
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 16;
        EditorGUILayout.LabelField("🎮 Unity Asset Optimizer", headerStyle);
        EditorGUILayout.LabelField("프로젝트 용량을 최적화합니다.", EditorStyles.miniLabel);
        EditorGUILayout.Space(10);
    }

    // ═══════════════════════════════════════════════════════════
    // 분석 섹션
    // ═══════════════════════════════════════════════════════════
    
    private void DrawAnalysisSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 프로젝트 분석", EditorStyles.boldLabel);
        
        if (GUILayout.Button("프로젝트 용량 분석", GUILayout.Height(30)))
        {
            AnalyzeProject();
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    // ═══════════════════════════════════════════════════════════
    // 텍스처 최적화 섹션
    // ═══════════════════════════════════════════════════════════
    
    private void DrawTextureSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🖼️ 텍스처 최적화", EditorStyles.boldLabel);
        
        _maxTextureSize = EditorGUILayout.IntPopup("최대 텍스처 크기", _maxTextureSize, 
            new string[] { "256", "512", "1024", "2048", "4096" },
            new int[] { 256, 512, 1024, 2048, 4096 });
        
        _useCrunchCompression = EditorGUILayout.Toggle("Crunch 압축 사용", _useCrunchCompression);
        
        if (_useCrunchCompression)
        {
            _crunchQuality = EditorGUILayout.IntSlider("압축 품질", _crunchQuality, 0, 100);
        }
        
        _generateMipmaps = EditorGUILayout.Toggle("3D용 Mipmap 생성", _generateMipmaps);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("선택한 텍스처 최적화"))
        {
            OptimizeSelectedTextures();
        }
        
        if (GUILayout.Button("모든 텍스처 최적화"))
        {
            if (EditorUtility.DisplayDialog("확인", 
                "모든 텍스처 설정을 변경합니다. 계속하시겠습니까?", "예", "아니오"))
            {
                OptimizeAllTextures();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    // ═══════════════════════════════════════════════════════════
    // 모델 최적화 섹션
    // ═══════════════════════════════════════════════════════════
    
    private void DrawModelSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🎭 모델 최적화", EditorStyles.boldLabel);
        
        _meshCompression = EditorGUILayout.Toggle("메시 압축", _meshCompression);
        _readWriteDisabled = EditorGUILayout.Toggle("Read/Write 비활성화", _readWriteDisabled);
        _optimizeMesh = EditorGUILayout.Toggle("메시 최적화", _optimizeMesh);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("선택한 모델 최적화"))
        {
            OptimizeSelectedModels();
        }
        
        if (GUILayout.Button("모든 모델 최적화"))
        {
            if (EditorUtility.DisplayDialog("확인", 
                "모든 모델 설정을 변경합니다. 계속하시겠습니까?", "예", "아니오"))
            {
                OptimizeAllModels();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    // ═══════════════════════════════════════════════════════════
    // 오디오 최적화 섹션
    // ═══════════════════════════════════════════════════════════
    
    private void DrawAudioSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🔊 오디오 최적화", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "• 짧은 효과음 (< 5초): Decompress On Load + ADPCM\n" +
            "• 긴 BGM (> 5초): Streaming + Vorbis 70%", 
            MessageType.Info);
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("모든 오디오 최적화"))
        {
            if (EditorUtility.DisplayDialog("확인", 
                "모든 오디오 설정을 변경합니다. 계속하시겠습니까?", "예", "아니오"))
            {
                OptimizeAllAudio();
            }
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    // ═══════════════════════════════════════════════════════════
    // 일괄 최적화 섹션
    // ═══════════════════════════════════════════════════════════
    
    private void DrawBatchOptimizeSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚡ 일괄 최적화", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "모든 에셋을 한 번에 최적화합니다.\n" +
            "⚠️ 시간이 오래 걸릴 수 있습니다!", 
            MessageType.Warning);
        
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("🚀 전체 프로젝트 최적화", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("최종 확인", 
                "모든 텍스처, 모델, 오디오를 최적화합니다.\n" +
                "이 작업은 되돌릴 수 없습니다. 계속하시겠습니까?", 
                "최적화 시작", "취소"))
            {
                OptimizeEntireProject();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    // ═══════════════════════════════════════════════════════════
    // 로그 섹션
    // ═══════════════════════════════════════════════════════════
    
    private void DrawLogSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📋 로그", EditorStyles.boldLabel);
        
        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = true;
        
        EditorGUILayout.TextArea(_logOutput, textAreaStyle, GUILayout.Height(150));
        
        if (GUILayout.Button("로그 지우기"))
        {
            _logOutput = "";
        }
        
        EditorGUILayout.EndVertical();
    }

    // ═══════════════════════════════════════════════════════════
    // 분석 기능
    // ═══════════════════════════════════════════════════════════
    
    private void AnalyzeProject()
    {
        _logOutput = "=== 프로젝트 분석 결과 ===\n\n";
        
        // 텍스처 분석
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        long totalTextureSize = 0;
        int largeTextureCount = 0;
        int uncompressedCount = 0;
        
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer != null)
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    totalTextureSize += fileInfo.Length;
                }
                
                if (importer.maxTextureSize > 1024)
                {
                    largeTextureCount++;
                }
                
                if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    uncompressedCount++;
                }
            }
        }
        
        _logOutput += $"📷 텍스처: {textureGuids.Length}개\n";
        _logOutput += $"   └ 전체 용량: {FormatSize(totalTextureSize)}\n";
        _logOutput += $"   └ 1024 초과: {largeTextureCount}개 (최적화 필요)\n";
        _logOutput += $"   └ 미압축: {uncompressedCount}개 (최적화 필요)\n\n";
        
        // 모델 분석
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
        long totalModelSize = 0;
        int readWriteEnabledCount = 0;
        
        foreach (string guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            
            if (importer != null)
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    totalModelSize += fileInfo.Length;
                }
                
                if (importer.isReadable)
                {
                    readWriteEnabledCount++;
                }
            }
        }
        
        _logOutput += $"🎭 모델: {modelGuids.Length}개\n";
        _logOutput += $"   └ 전체 용량: {FormatSize(totalModelSize)}\n";
        _logOutput += $"   └ Read/Write 활성: {readWriteEnabledCount}개 (최적화 가능)\n\n";
        
        // 오디오 분석
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });
        long totalAudioSize = 0;
        
        foreach (string guid in audioGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                totalAudioSize += fileInfo.Length;
            }
        }
        
        _logOutput += $"🔊 오디오: {audioGuids.Length}개\n";
        _logOutput += $"   └ 전체 용량: {FormatSize(totalAudioSize)}\n\n";
        
        // 총합
        long totalSize = totalTextureSize + totalModelSize + totalAudioSize;
        _logOutput += $"━━━━━━━━━━━━━━━━━━━━━━━━\n";
        _logOutput += $"📦 총 에셋 용량: {FormatSize(totalSize)}\n";
        _logOutput += $"\n💡 '전체 프로젝트 최적화'로 용량을 줄여보세요!\n";
        
        Debug.Log(_logOutput);
    }

    // ═══════════════════════════════════════════════════════════
    // 텍스처 최적화
    // ═══════════════════════════════════════════════════════════
    
    private void OptimizeSelectedTextures()
    {
        Object[] selectedObjects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
        
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("알림", "Project 창에서 텍스처를 선택해주세요.", "확인");
            return;
        }
        
        int optimizedCount = 0;
        
        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (OptimizeTexture(path))
            {
                optimizedCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        _logOutput += $"✅ {optimizedCount}개 텍스처 최적화 완료\n";
    }
    
    private void OptimizeAllTextures()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        int optimizedCount = 0;
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            
            if (EditorUtility.DisplayCancelableProgressBar("텍스처 최적화 중...", 
                Path.GetFileName(path), (float)i / guids.Length))
            {
                break;
            }
            
            if (OptimizeTexture(path))
            {
                optimizedCount++;
            }
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        _logOutput += $"✅ {optimizedCount}/{guids.Length}개 텍스처 최적화 완료\n";
    }
    
    private bool OptimizeTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return false;
        
        bool changed = false;
        
        // 최대 크기 설정
        if (importer.maxTextureSize > _maxTextureSize)
        {
            importer.maxTextureSize = _maxTextureSize;
            changed = true;
        }
        
        // 압축 설정
        if (importer.textureCompression != TextureImporterCompression.Compressed)
        {
            importer.textureCompression = TextureImporterCompression.Compressed;
            changed = true;
        }
        
        // Crunch 압축
        if (_useCrunchCompression && !importer.crunchedCompression)
        {
            importer.crunchedCompression = true;
            importer.compressionQuality = _crunchQuality;
            changed = true;
        }
        
        // Mipmap 설정
        if (importer.textureType != TextureImporterType.Sprite && 
            importer.textureType != TextureImporterType.GUI)
        {
            // 3D 오브젝트용 텍스처: Mipmap 활성화
            if (importer.mipmapEnabled != _generateMipmaps)
            {
                importer.mipmapEnabled = _generateMipmaps;
                changed = true;
            }
        }
        else
        {
            // UI/스프라이트: Mipmap 비활성화
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }
        }
        
        if (changed)
        {
            importer.SaveAndReimport();
        }
        
        return changed;
    }

    // ═══════════════════════════════════════════════════════════
    // 모델 최적화
    // ═══════════════════════════════════════════════════════════
    
    private void OptimizeSelectedModels()
    {
        Object[] selectedObjects = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);
        
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("알림", "Project 창에서 모델을 선택해주세요.", "확인");
            return;
        }
        
        int optimizedCount = 0;
        
        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (OptimizeModel(path))
            {
                optimizedCount++;
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        _logOutput += $"✅ {optimizedCount}개 모델 최적화 완료\n";
    }
    
    private void OptimizeAllModels()
    {
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
        int optimizedCount = 0;
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            
            if (EditorUtility.DisplayCancelableProgressBar("모델 최적화 중...", 
                Path.GetFileName(path), (float)i / guids.Length))
            {
                break;
            }
            
            if (OptimizeModel(path))
            {
                optimizedCount++;
            }
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        _logOutput += $"✅ {optimizedCount}/{guids.Length}개 모델 최적화 완료\n";
    }
    
    private bool OptimizeModel(string path)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) return false;
        
        bool changed = false;
        
        // 메시 압축
        if (_meshCompression && importer.meshCompression != ModelImporterMeshCompression.Medium)
        {
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            changed = true;
        }
        
        // Read/Write 비활성화 (런타임 메시 수정이 필요 없는 경우)
        if (_readWriteDisabled && importer.isReadable)
        {
            importer.isReadable = false;
            changed = true;
        }
        
        // 메시 최적화
        if (_optimizeMesh)
        {
            if (!importer.optimizeMeshVertices)
            {
                importer.optimizeMeshVertices = true;
                changed = true;
            }
            if (!importer.optimizeMeshPolygons)
            {
                importer.optimizeMeshPolygons = true;
                changed = true;
            }
        }
        
        if (changed)
        {
            importer.SaveAndReimport();
        }
        
        return changed;
    }

    // ═══════════════════════════════════════════════════════════
    // 오디오 최적화
    // ═══════════════════════════════════════════════════════════
    
    private void OptimizeAllAudio()
    {
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });
        int optimizedCount = 0;
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            
            if (EditorUtility.DisplayCancelableProgressBar("오디오 최적화 중...", 
                Path.GetFileName(path), (float)i / guids.Length))
            {
                break;
            }
            
            if (OptimizeAudio(path))
            {
                optimizedCount++;
            }
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        _logOutput += $"✅ {optimizedCount}/{guids.Length}개 오디오 최적화 완료\n";
    }
    
    private bool OptimizeAudio(string path)
    {
        AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
        if (importer == null) return false;
        
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) return false;
        
        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        bool changed = false;
        
        // 5초 기준으로 설정 분리
        if (clip.length < 5f)
        {
            // 짧은 효과음
            if (settings.loadType != AudioClipLoadType.DecompressOnLoad ||
                settings.compressionFormat != AudioCompressionFormat.ADPCM)
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.ADPCM;
                changed = true;
            }
        }
        else
        {
            // 긴 BGM
            if (settings.loadType != AudioClipLoadType.Streaming ||
                settings.compressionFormat != AudioCompressionFormat.Vorbis)
            {
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.7f;
                changed = true;
            }
        }
        
        if (changed)
        {
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
        
        return changed;
    }

    // ═══════════════════════════════════════════════════════════
    // 전체 최적화
    // ═══════════════════════════════════════════════════════════
    
    private void OptimizeEntireProject()
    {
        _logOutput = "=== 전체 프로젝트 최적화 시작 ===\n";
        _logOutput += $"시작 시간: {System.DateTime.Now:HH:mm:ss}\n\n";
        
        OptimizeAllTextures();
        _logOutput += "\n";
        
        OptimizeAllModels();
        _logOutput += "\n";
        
        OptimizeAllAudio();
        
        _logOutput += $"\n=== 최적화 완료! ===\n";
        _logOutput += $"종료 시간: {System.DateTime.Now:HH:mm:ss}\n";
        _logOutput += $"\n💡 File → Build Settings → Build로\n";
        _logOutput += $"   빌드하여 용량 변화를 확인하세요!\n";
        
        EditorUtility.DisplayDialog("완료", 
            "전체 프로젝트 최적화가 완료되었습니다!\n\n" +
            "빌드하여 용량 변화를 확인해보세요.", "확인");
    }

    // ═══════════════════════════════════════════════════════════
    // 유틸리티
    // ═══════════════════════════════════════════════════════════
    
    private string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }
}
#endif

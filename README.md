# SurfRush

3D-аркада на Unity 6 (URP) про сёрфинг от третьего лица. Игрок едет на доске
по бесконечному океану и уворачивается от препятствий, плотность которых
растёт со временем.

Главное в проекте — физическая модель воды. Волны Герстнера считаются на CPU
и пересчитывают вершины меша океана каждый кадр. Та же функция высоты
сэмплируется физикой доски, поэтому доска не «играет анимацию плавания», а
действительно кренится на склонах волн и ускоряется под действием спроецированной
гравитации.

Unity `6000.3.8f1`, URP, C#, New Input System, Cinemachine 3, PhysX, Shader Graph.

## Геймплей

- Управление: `A`/`D` или стрелки (поворот), `Space` (буст), `ESC` (пауза).
  Поддержан и геймпад через Input Action Asset.
- Очки начисляются за каждый пройденный метр вперёд с растущим множителем
  комбо. Множитель сбрасывается при ударе.
- 3 жизни. После удара 1.5 секунды i-frames, чтобы один контакт не съел все
  жизни сразу.
- Лучший результат хранится в `PlayerPrefs`.
- В главном меню можно выбрать День или Вечер. Выбор меняет skybox, цвет
  солнца и набор препятствий (пальмы и рифы для дня, лодки и обломки кораблей
  для вечера).

## Как устроена вода

`WaveField` — статический сэмплер высоты и нормали воды. Внутри суммируются
несколько волн Герстнера: для каждой считается `omega = sqrt(g · k)`, фаза,
горизонтальное схождение точек к гребню через коэффициент steepness. Это
единственная точка, где живёт формула волны.

Меш океана (`OceanMeshChunk`) в `LateUpdate` сэмплирует `WaveField` для каждой
своей вершины в мировых координатах и записывает результат обратно в `Mesh`.
Поскольку сэмплинг идёт в мировом пространстве, соседние чанки автоматически
сходятся на швах без отдельной логики стыковки.

Бесконечный океан реализован через `OceanChunkManager`: вокруг игрока
поддерживается сетка 3×3 чанков, при пересечении границы чанк не пересоздаётся,
а сдвигается на новое место.

## Физика доски

Доска — обычный `Rigidbody`. Два скрипта в `FixedUpdate` дают ей всё поведение.

`SurfboardBuoyancy` хранит несколько локальных якорей (4 угла + центр). Для
каждого якоря сэмплируется высота воды, считается погружение
`depth = waterY - anchorY` и прикладывается вертикальная сила
`Vector3.up * strength * depth` через `AddForceAtPosition`. Разные точки
чувствуют разную высоту, поэтому доска кренится сама — без явного управления
ротацией.

`SurfboardSlopePropulsion` берёт нормаль воды под центром доски и проецирует
гравитацию на касательную плоскость волны:

```csharp
Vector3 slopeForce = Vector3.ProjectOnPlane(Physics.gravity, waterNormal) * mul;
_rb.AddForce(slopeForce, ForceMode.Acceleration);
```

На спуске со склона компонента направлена вперёд → ускорение. На подъёме к
гребню — назад → торможение. Дополнительно тут считается раздельный drag по
forward/lateral осям и стабилизация ориентации по нормали воды через
`AddTorque`. Никакого ручного выставления скорости.

## Препятствия и сложность

`ObstacleSpawner` каждые `spawnInterval` секунд спавнит порцию препятствий
впереди игрока на `Z + spawnAheadDistance`. X выбирается случайно в коридоре
с rejection sampling по минимальной дистанции до уже стоящих препятствий —
это даёт разреженное, не слипающееся распределение. Объекты за спиной
возвращаются в `ObstaclePool`.

Сложность хранится в `DifficultyProfile` (ScriptableObject) как набор
`AnimationCurve`: амплитуда волн, скорость волн, интервал спавна, количество
препятствий в порции, ширина коридора. `DifficultyController` каждый кадр
сэмплит кривые от `playTime` и применяет результат к `WaveField` и
`ObstacleSpawner`. Балансировка кривых делается прямо в Editor, без
перекомпиляции.

Тематический набор препятствий выбирается через `ObstacleTheme` — SO со
списком префабов и весами. Спавнер просит у темы случайный префаб, тема
делает взвешенный выбор. Так День и Вечер используют один и тот же
ObstacleSpawner с разными темами.

## Структура проекта

```
Assets/_Game/
├── Art/                модели, текстуры, материалы, шейдер воды
├── Audio/              музыка и SFX
├── Prefabs/            доска, чанк океана, препятствия
├── Scenes/             Menu.unity, Main.unity
├── ScriptableObjects/
│   ├── WaveProfiles/
│   ├── DifficultyProfiles/
│   ├── ObstacleThemes/     Theme_Day, Theme_Evening
│   ├── TimeOfDay/          Preset_Day, Preset_Evening
│   └── Surfboard/          SurfboardConfig
├── Settings/           URP-ассеты, Input Action Asset
└── Scripts/
    ├── Core/           GameManager, ScoreSystem, AudioManager,
    │                   GameSettings, TimeOfDayApplier, TimeOfDayPreset
    ├── Ocean/          GerstnerWave, WaveProfile, WaveField,
    │                   OceanMeshChunk, OceanChunkManager, WaveProbe
    ├── Player/         SurfboardBuoyancy, SurfboardSlopePropulsion,
    │                   SurfboardController, SurfboardCollisionDetector,
    │                   SurfboardLateralBound, SurfboardConfig,
    │                   SimpleFollowCamera
    ├── Spawn/          Obstacle, ObstaclePool, ObstacleSpawner, ObstacleTheme
    ├── Difficulty/     DifficultyProfile, DifficultyController
    └── UI/             HudController, MenuController, PauseController
```

Один `SurfRush.asmdef` в корне `Scripts/` — компилируется быстро.
Namespaces по папкам: `SurfRush.Ocean`, `SurfRush.Player`, `SurfRush.Spawn`,
`SurfRush.Difficulty`, `SurfRush.Core`, `SurfRush.UI`.

## Запуск

1. Установить Unity Hub и Unity `6000.3.8f1`.
2. Открыть проект через Unity Hub (Add → выбрать папку `MaiGame`).
3. Дождаться импорта пакетов. Если выскочит окно «Convert to URP» — согласиться.
4. Открыть `Assets/_Game/Scenes/Menu.unity`.
5. В Build Settings проверить, что Menu и Main добавлены (Menu — индекс 0).
6. Play.

## Ассеты

Все сторонние модели, текстуры и звуки перечислены в [CREDITS.md](CREDITS.md)
вместе с авторами, лицензиями и ссылками на источники.

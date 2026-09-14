ShowLandMines（增强版）
一个用于 SPT5.0.0 BE 的 BepInEx 插件，可以可视化地图上的雷区与狙击 AI 区域，并且能一键禁用/启用雷区和狙击区。

本插件基于原版 ShowLandMines 修改而来，修复了原版在 IL2CPP 环境下无法真正禁用雷区/狙击区的问题，并修改了按键操作。

功能特性
雷区可视化：以红色半透明方块显示地图中的所有雷区。

狙击区可视化：以蓝色半透明方块显示所有狙击 AI 触发区域。

一键禁用/启用：可随时关闭或开启所有雷区与狙击区的伤害/触发（雷区不再爆炸，狙击区不再开枪）。

按键操作：所有功能均可通过快捷键切换，不影响正常游戏。

去除了原版空气墙可视化的功能
新增禁用雷区/狙击区的功能
禁用方式	—	直接禁用场景组件与碰撞器，确保在 IL2CPP 下生效
本地玩家判断	通过对象名 PlayerSuperior(Clone)	使用 Player.IsYourPlayer，
重要说明：原版若尝试通过 Harmony Patch 游戏方法（如 Minefield.Explode）来禁用雷区，在 IL2CPP 环境下往往无效，因为游戏原生逻辑不经过 C# 桥接方法。本增强版改为直接遍历场景，禁用 Minefield、MineDirectionalColliders、SniperFiringZone 组件及其 Collider，从根本上阻止触发，保证功能生效。

安装方法
确保你的 SPT 版本为 5.0.0，并已安装 BepInEx 6 (IL2CPP)。

下载编译好的 ShowLandMines.dll。

将 ShowLandMines.dll 放入 BepInEx/plugins/ 目录。

启动游戏，插件会自动加载。

使用方法
进入战局后，使用以下快捷键：

快捷键	功能
Ctrl + 小键盘 7	显示 / 隐藏雷区
Ctrl + 小键盘 8	显示 / 隐藏狙击区
Ctrl + 小键盘 9	禁用 / 启用所有雷区和狙击区

再次按下同一快捷键即可恢复。

🔧 从源码构建
已安装的 SPT，路径需在 VegetationRemover.csproj 的 <TarkovDir> 里配好（默认 D:\SPT-5.0.0-47242-BE\）

使用
build.bat

致谢
原版 ShowLandMines 作者 www.bilibili.com/video/BV1mmrNY6Eh2

所有 SPT 社区贡献者。

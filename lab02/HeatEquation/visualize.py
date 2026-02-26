# Визуализация результатов моделирования теплопроводности
# Читает CSV из C#-программы и показывает анимацию с паузой и слайдером
#
# Запуск: python visualize.py путь/к/snapshots.csv
# CSV формат:
#   Строка 1: Tmin,Tmax,длина_пластины,общее_время,кол-во_кадров
#   Строки 2+: температуры в каждой точке сетки через запятую

import sys, os
sys.stderr = open(os.devnull, "w")  # подавляем ошибки matplotlib
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.colors as mcolors
from matplotlib.animation import FuncAnimation
from matplotlib.widgets import Button, Slider

# Читаем CSV-файл, путь передан аргументом командной строки
csv_path = sys.argv[1]
lines = open(csv_path).read().splitlines()

# Первая строка — метаданные для настройки визуализации
meta = [float(v) for v in lines[0].split(",")]
temp_min, temp_max, plate_length, total_time = meta[0], meta[1], meta[2], meta[3]

# Остальные строки — снимки температуры (каждая строка = распределение температуры по пластине в один момент времени)
frames = [np.array([float(v) for v in line.split(",")]) for line in lines[1:]]

# Создаём фигуру с отступом снизу для слайдера и кнопки
fig, ax = plt.subplots(figsize=(10, 3.5))
plt.subplots_adjust(bottom=0.25)

# imshow отображает одномерный массив температур как цветную полосу
# coolwarm: синий = холод, красный = тепло
# extent задаёт реальные координаты осей (метры)
# Центрируем цветовую шкалу на 0°C: ниже нуля — синий, выше — красный
norm = mcolors.TwoSlopeNorm(vmin=temp_min, vcenter=0, vmax=temp_max)
heatmap = ax.imshow(
    [frames[0]], aspect="auto", cmap="coolwarm",
    norm=norm, extent=[0, plate_length, 0, 1]
)
ax.set_yticks([])                # Y-ось не нужна (полоса одномерная)
ax.set_xlabel("x, м")

# Цветовая шкала с явными отметками на границах диапазона
cbar = fig.colorbar(heatmap, ax=ax, label="T, °C")
cbar.set_ticks([temp_min, temp_min/2, 0, temp_max])

# Заголовок показывает текущее время моделирования
title = ax.set_title("t = 0.000 с", fontsize=14, fontweight="bold")

# Слайдер для ручной перемотки по кадрам
ax_slider = plt.axes([0.12, 0.08, 0.6, 0.04])
slider = Slider(ax_slider, "Кадр", 0, len(frames) - 1, valinit=0, valstep=1)

# Кнопка паузы/воспроизведения
ax_btn = plt.axes([0.78, 0.08, 0.1, 0.05])
btn = Button(ax_btn, "Пауза")

paused = [False]
current = [0]           # текущий кадр — управляем вручную
slider_dragging = [False]  # флаг: пользователь двигает слайдер

def show_frame(i):
    """Отрисовать кадр i"""
    heatmap.set_data([frames[i]])
    title.set_text(f"t = {i * total_time / len(frames):.3f} с")

# При перемещении слайдера — показываем выбранный кадр и запоминаем позицию
def on_slider(val):
    current[0] = int(val)
    show_frame(current[0])
    fig.canvas.draw_idle()

# Переключение паузы
def toggle_pause(event):
    paused[0] = not paused[0]
    btn.label.set_text("Запуск" if paused[0] else "Пауза    ")

# Генератор кадров — продвигает счётчик только когда не на паузе
# FuncAnimation вызывает его каждые 50 мс, но мы сами решаем какой кадр показать
def frame_gen():
    while True:
        if not paused[0]:
            current[0] = (current[0] + 1) % len(frames)
        yield current[0]

# Функция анимации — обновляет картинку и слайдер
def update(i):
    show_frame(i)
    slider.set_val(i)
    return heatmap, title

slider.on_changed(on_slider)
btn.on_clicked(toggle_pause)

# anim нужно сохранить в переменную, иначе Python удалит объект и анимация не заработает
anim = FuncAnimation(fig, update, frames=frame_gen, interval=50, blit=False, save_count=len(frames))
plt.show()

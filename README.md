# ShoushiLing

## To Make Animated Button Sprites

Go to character scene and import character
Click on Gif Tab
Play Scene
(Reset Gif recording if needed)
Set size to 1000 x 1000
Set transparency color
Set Frames to 30 (for 30 frame animation)
Click Record

Set First Frame to 1 and last frame to 30
Save Gif

```
convert animation.gif target.png
```
```
mogrify -resize 30% *
```

Import images to TexturePacker
Choose Unitframework
Choose "Grid/Strip" Algorithm
Choose Max Size: 2048
Publish Sprite


```
# SEE: https://gist.github.com/dergachev/4627207
ffmpeg -i Capture.mov -s 600x400 -pix_fmt rgb24 -r 10 -f gif - | gifsicle --optimize=3 --delay=3 > Capture.gif
```
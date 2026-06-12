import cv2

print('Apply adaptive threshold')

image_src = cv2.imread("test_adaptive_rgb.bmp")

# Convert to grayscale
gray = cv2.cvtColor(image_src, cv2.COLOR_BGR2GRAY)

# Apply adaptive thresholding
binary_picture = cv2.adaptiveThreshold(gray,255,cv2.ADAPTIVE_THRESH_MEAN_C, cv2.THRESH_BINARY,11,3)

cv2.imshow("image_src", image_src)
cv2.imshow("binary_picture", binary_picture)

cv2.waitKey(0)
cv2.destroyAllWindows()
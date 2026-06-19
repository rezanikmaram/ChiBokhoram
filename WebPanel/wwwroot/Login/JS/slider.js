const imageUrls = [
    'login/images/img1.jpg',
    'login/images/img2.jpg',
    'login/images/img3.jpg',
    'login/images/img4.jpg'

  
  ];
  
  const divElement = document.getElementById('side-pic');
  let currentImageIndex = 0;
  
function changeBackgroundImage() {
    divElement.style.backgroundImage = `url(${imageUrls[currentImageIndex]})`;
    currentImageIndex = (currentImageIndex + 1) % imageUrls.length;
  }
  
  setInterval(changeBackgroundImage, 2000);
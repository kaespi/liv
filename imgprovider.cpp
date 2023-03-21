#include "imgprovider.h"

#include <QDir>

ImgProvider::ImgProvider() : QQuickImageProvider(QQuickImageProvider::Pixmap) {}

QPixmap ImgProvider::requestPixmap(const QString &id, QSize *size,
                                   const QSize &requestedSize) {
  qDebug() << "requestPixmap(" << id << ")";
  qDebug() << QDir::currentPath();

  int width = 100;
  int height = 50;

  QPixmap pixmap;
  if (id == "1") {
    pixmap.load("../lightweightimageviewer/data/testimages/bbv1.png");
  } else if (id == "2") {
    pixmap.load("../lightweightimageviewer/data/testimages/bbv2.jpg");
  } else {
    pixmap =
        QPixmap(requestedSize.width() > 0 ? requestedSize.width() : width,
                requestedSize.height() > 0 ? requestedSize.height() : height);
    pixmap.fill(QColor(id).rgba());
  }

  *size = pixmap.size();

  return pixmap;
}

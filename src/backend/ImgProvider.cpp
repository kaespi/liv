#include "ImgProvider.h"

#include <QDir>
#include <QFile>

ImgProvider::ImgProvider(const QString &file)
    : QQuickImageProvider(QQuickImageProvider::Pixmap) {
  if (not file.isEmpty() && QFile::exists(file)) {
    m_file = file;
  }
}

QPixmap ImgProvider::requestPixmap(const QString &id, QSize *size,
                                   const QSize &requestedSize) {
  qDebug() << "requestPixmap(" << id << ")";
  qDebug() << QDir::currentPath();

  int width = 100;
  int height = 50;

  QPixmap pixmap;
  if (id == "1") {
    if (not m_file.isEmpty()) {
      pixmap.load(m_file);
    } else {
      pixmap.load("../../lightweightimageviewer/data/testimages/bbv1.png");
    }
  } else {
    pixmap =
        QPixmap(requestedSize.width() > 0 ? requestedSize.width() : width,
                requestedSize.height() > 0 ? requestedSize.height() : height);
    pixmap.fill(QColor(id).rgba());
  }

  *size = pixmap.size();

  return pixmap;
}

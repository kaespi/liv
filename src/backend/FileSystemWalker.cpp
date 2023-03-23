#include "FileSystemWalker.h"

#include <QDir>
#include <QFile>
#include <QFileInfo>

namespace {
QString composeFullPath(const QDir &dir, const QString &filename) {
  return QDir::cleanPath(dir.absolutePath() + QDir::separator() + filename);
}

void collectFilesInDir(const QDir &dir, QStringList &files) {
  files = dir.entryList(QStringList() << "*.jpg"
                                      << "*.JPG"
                                      << "*.png"
                                      << "*.PNG",
                        QDir::Files);
  files.sort(Qt::CaseSensitivity::CaseInsensitive);
}
} // namespace

FileSystemWalker::FileSystemWalker(const QString &file) {
  if (file.isEmpty()) {
    qWarning("No filename provided");
    m_dir = QDir(QDir::currentPath());
  } else {
    if (QFile::exists(file)) {
      QFileInfo fileInfo(file);
      if (fileInfo.isFile()) {
        auto filename = fileInfo.fileName();
        auto dir = fileInfo.dir();
        m_dir = dir;
        collectFilesInDir(m_dir, m_filesInDir);
        m_currentFileIndex = m_filesInDir.indexOf(filename);
      } else {
        m_dir = QDir(file);
        collectFilesInDir(m_dir, m_filesInDir);
      }
    } else {
      m_dir = QDir(QDir::currentPath());
      qWarning("File %s doesn't exist", file.toLatin1().data());
    }
  }
}

QString FileSystemWalker::getCurrentFile() const {
  return composeFullPath(m_dir, m_filesInDir[m_currentFileIndex]);
}

QString FileSystemWalker::getNextFile() {
  ++m_currentFileIndex;
  if (m_currentFileIndex >= m_filesInDir.size()) {
    m_currentFileIndex = 0;
  }
  return composeFullPath(m_dir, m_filesInDir[m_currentFileIndex]);
}

QString FileSystemWalker::getPrevFile() {
  --m_currentFileIndex;
  if (m_currentFileIndex < 0) {
    m_currentFileIndex = m_filesInDir.size() - 1;
  }
  return composeFullPath(m_dir, m_filesInDir[m_currentFileIndex]);
}
